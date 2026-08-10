import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { CHANNEL_ICONS, BroadcastDetail } from '../../../models/admin-broadcast.model';
import { BroadcastService } from '../../../services/admin-broadcast.service';

@Component({
  selector: 'app-broadcast-detail-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatDialogModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './broadcast-detail-modal.component.html',
  styleUrls: ['./broadcast-detail-modal.component.css']
})
export class BroadcastDetailModalComponent implements OnInit {
  private readonly svc = inject(BroadcastService);
  private readonly dialogData = inject(MAT_DIALOG_DATA);
  
  CHANNEL_ICONS = CHANNEL_ICONS;
  loading = signal(true);
  data = signal<BroadcastDetail | null>(null);

  ngOnInit(): void {
    const id = this.dialogData?.id;
    if (!id) return;

    this.svc.getBroadcastById(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.data.set(res.data);
          }
        },
        error: (err) => console.error('Failed to load details', err)
      });
  }
}