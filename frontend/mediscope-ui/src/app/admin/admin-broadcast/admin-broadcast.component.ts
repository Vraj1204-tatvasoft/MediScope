import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDivider } from '@angular/material/divider';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, debounceTime, distinctUntilChanged, Subscription } from 'rxjs';

import { BroadcastService } from '../../services/admin-broadcast.service';
import { GenericConfirmModalComponent, ConfirmDialogData } from '../generic-confirm-modal/generic-confirm-modal.component';
import {
  BroadcastListItem,
  BroadcastStatus,
  CHANNEL_LABELS,
  CHANNEL_ICONS,
  AUDIENCE_LABELS,
} from '../../models/admin-broadcast.model';
import { BroadcastFormModalComponent } from '../broadcast-form-modal/broadcast-form-modal.component';
import { BroadcastDetailModalComponent } from './broadcast-detail-modal/broadcast-detail-modal.component';
import { SignalrService } from '../../services/signalr.service';

@Component({
  selector: 'app-admin-broadcast',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatTooltipModule,
    MatDivider,
  ],
  templateUrl: './admin-broadcast.component.html',
  styleUrls: ['./admin-broadcast.component.css'],
})
export class AdminBroadcastComponent implements OnInit {
  private readonly svc    = inject(BroadcastService);
  private readonly dialog = inject(MatDialog);
  private signalrService = inject(SignalrService);
  private signalrSubscriptions = new Subscription();
  broadcasts  = signal<BroadcastListItem[]>([]);
  loading     = signal(false);
  totalCount  = signal(0);
  actioningId = signal<string | null>(null);

  searchControl = new FormControl('');
  searchTerm    = signal('');
  statusFilter  = signal<BroadcastStatus | null>(null);

  // Expose to template
  CHANNEL_LABELS  = CHANNEL_LABELS;
  CHANNEL_ICONS   = CHANNEL_ICONS;
  AUDIENCE_LABELS = AUDIENCE_LABELS;
  Math            = Math;

  readonly statusFilters: { label: string; value: BroadcastStatus | null }[] = [
    { label: 'All',        value: null },
    { label: 'Draft',      value: 'Draft' },
    { label: 'Pending',    value: 'Pending' },
    { label: 'Processing', value: 'Processing' },
    { label: 'Completed',  value: 'Completed' },
    { label: 'Failed',     value: 'Failed' },
  ];

  filteredBroadcasts = computed(() => {
    const term   = this.searchTerm().toLowerCase().trim();
    const status = this.statusFilter();
    return this.broadcasts().filter(b => {
      const matchesTerm   = !term   || b.name.toLowerCase().includes(term);
      const matchesStatus = !status || b.status === status;
      return matchesTerm && matchesStatus;
    });
  });

  constructor() {
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(),
    ).subscribe(val => this.searchTerm.set(val || ''));
  }

  ngOnInit(): void { this.load();
    this.listenToSignalREvents();
   }
   ngOnDestroy(): void {
    this.signalrSubscriptions.unsubscribe();
  }
  load(): void {
    this.loading.set(true);
    this.svc.getBroadcasts({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: res => {
        this.broadcasts.set(res.data?.items ?? []);
        this.totalCount.set(res.data?.totalCount ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
  private listenToSignalREvents(): void {
    this.signalrSubscriptions.add(
      this.signalrService.broadcastStatus$.subscribe((data: any) => {
        const broadcastId = data.broadcastId ?? data.BroadcastId;
        const status = data.status ?? data.Status;
        const totalSent = data.totalSent ?? data.TotalSent;
        const totalFailed = data.totalFailed ?? data.TotalFailed;

        this.broadcasts.update((list) =>
          list.map((b) => {
            if (b.id === broadcastId) {
              return {
                ...b,
                status: status,
                statusDisplay: status,
                ...(totalSent !== undefined && { sentCount: totalSent }),
                ...(totalFailed !== undefined && { failedCount: totalFailed }),
              };
            }
            return b;
          })
        );
      })
    );

    this.signalrSubscriptions.add(
      this.signalrService.broadcastProgress$.subscribe((data: any) => {
        const broadcastId = data.broadcastId ?? data.BroadcastId;
        const sentDelta = data.sent ?? data.Sent ?? 0;
        const failedDelta = data.failed ?? data.Failed ?? 0;
        const isRetry = data.isRetry ?? data.IsRetry ?? false;

        this.broadcasts.update((list) =>
          list.map((b) => {
            if (b.id === broadcastId) {
              return {
                ...b,
                sentCount: (b.sentCount || 0) + sentDelta,
                failedCount: isRetry
                  ? Math.max(0, (b.failedCount || 0) - sentDelta + failedDelta) 
                  : (b.failedCount || 0) + failedDelta,
              };
            }
            return b;
          })
        );
      })
    );
  }
  setStatusFilter(value: BroadcastStatus | null): void {
    this.statusFilter.set(value);
  }

  openCreateModal(): void {
    const ref = this.dialog.open(BroadcastFormModalComponent, {
      width: '580px', disableClose: true, data: null,
    });
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  // Edit

  openEditModal(b: BroadcastListItem): void {
    this.actioningId.set(b.id);
    this.svc.getBroadcastById(b.id)
      .pipe(finalize(() => this.actioningId.set(null)))
      .subscribe({
        next: res => {
          if (!res.data) return;
          const ref = this.dialog.open(BroadcastFormModalComponent, {
            width: '580px', disableClose: true, data: res.data,
          });
          ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
        },
      });
  }

  //  Send 

  sendBroadcast(b: BroadcastListItem): void {
    const ref = this.dialog.open<GenericConfirmModalComponent, ConfirmDialogData>(
      GenericConfirmModalComponent, {
        width: '420px',
        data: {
          title:   'Send Broadcast',
          message: `Send "${b.name}" to ${AUDIENCE_LABELS[b.audience]}? This will queue the broadcast immediately.`,
          confirmText: 'Send',
          theme: 'primary',
          icon: 'send'
        },
      }
    );
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.actioningId.set(b.id);
      this.svc.sendBroadcast(b.id)
        .pipe(finalize(() => this.actioningId.set(null)))
        .subscribe({ next: () => this.load() });
    });
  }

  //  Retry 

  retryBroadcast(b: BroadcastListItem): void {
    const ref = this.dialog.open<GenericConfirmModalComponent, ConfirmDialogData>(
      GenericConfirmModalComponent, {
        width: '420px',
        data: {
          title:   'Retry Failed Recipients',
          message: `Retry "${b.name}" for ${b.failedCount} failed recipient(s)? Successful deliveries will not be re-sent.`,
          confirmText: 'Retry',
          theme: 'warning',
          icon: 'refresh'
        },
      }
    );
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.actioningId.set(b.id);
      this.svc.retryBroadcast(b.id)
        .pipe(finalize(() => this.actioningId.set(null)))
        .subscribe({ next: () => this.load() });
    });
  }
  
  openViewModal(b: BroadcastListItem): void {
    this.dialog.open(BroadcastDetailModalComponent, {
      width: '600px',
      data: { id: b.id }
    });
  }
  //  Delete 

  openDeleteModal(b: BroadcastListItem): void {
    const ref = this.dialog.open<GenericConfirmModalComponent, ConfirmDialogData>(
      GenericConfirmModalComponent, {
        width: '420px',
        data: {
          title:   'Delete Broadcast',
          message: `Are you sure you want to delete "${b.name}"? This cannot be undone.`,
          confirmText: 'Delete',
          theme: 'danger',
          icon: 'delete'
        },
      }
    );
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.svc.deleteBroadcast(b.id).subscribe({ next: () => this.load() });
    });
  }
  
  //  Visibility helpers — string comparisons, no casting needed 

  canEdit(b: BroadcastListItem): boolean {
    return b.status === 'Draft';
  }

  canSend(b: BroadcastListItem): boolean {
    return b.status === 'Draft' || b.status === 'Failed';
  }

  canRetry(b: BroadcastListItem): boolean {
    return (b.status === 'Completed' || b.status === 'Failed') && b.failedCount > 0;
  }

  canDelete(b: BroadcastListItem): boolean {
    return b.status === 'Draft' || b.status === 'Failed';
  }

  isProcessing(b: BroadcastListItem): boolean {
    return b.status === 'Pending' || b.status === 'Processing';
  }

  trackById(_index: number, b: BroadcastListItem): string { return b.id; }
}