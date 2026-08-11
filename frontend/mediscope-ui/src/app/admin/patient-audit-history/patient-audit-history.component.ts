import {Component, Inject, OnDestroy, OnInit, computed, inject, signal} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

import {
  Subject,
  Subscription
} from 'rxjs';

import {
  debounceTime,
  distinctUntilChanged
} from 'rxjs/operators';

import {
  PatientAuditHistory
} from '../../models/patient-audit-history.model';

import {
  PatientAuditHistoryService
} from '../../services/patient-audit-history.service';

@Component({
  selector: 'app-patient-audit-history',
  standalone: true,
  templateUrl: './patient-audit-history.component.html',
  styleUrls: ['./patient-audit-history.component.css'],
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatDividerModule
  ]
})
export class PatientAuditHistoryComponent
  implements OnInit, OnDestroy {

  private readonly service = inject(PatientAuditHistoryService);

  readonly displayedColumns = [
    'changedAt',
    'fieldName',
    'oldValue',
    'newValue',
    'changedBy'
  ];

  readonly loading = signal(true);

  readonly search = signal('');

  readonly pageNumber = signal(1);

  readonly pageSize = signal(10);

  readonly totalRecords = signal(0);

  readonly auditLogs = signal<PatientAuditHistory[]>([]);

  readonly totalPages = computed(() =>
    Math.ceil(this.totalRecords() / this.pageSize())
  );

  private readonly searchSubject = new Subject<string>();

  private searchSubscription?: Subscription;

  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: {
      patientId: string;
      patientName: string;
    },
    private dialogRef: MatDialogRef<PatientAuditHistoryComponent>
  ) {}

  ngOnInit(): void {

    this.searchSubscription = this.searchSubject
      .pipe(
        debounceTime(400),
        distinctUntilChanged()
      )
      .subscribe(value => {
        this.search.set(value);
        this.pageNumber.set(1);
        this.loadAuditHistory();
      });

    this.loadAuditHistory();
  }

  loadAuditHistory(): void {

    this.loading.set(true);

    this.service
      .getAuditHistory(
        this.data.patientId,
        this.pageNumber(),
        this.pageSize(),
        this.search()
      )
      .subscribe({
        next: response => {

          this.auditLogs.set(response.data.items);

          this.totalRecords.set(response.data.totalCount);

          this.loading.set(false);

        },
        error: () => {

          this.auditLogs.set([]);

          this.loading.set(false);

        }
      });

  }

  onSearch(event: Event): void {

    const value = (event.target as HTMLInputElement).value;

    this.searchSubject.next(value);

  }

  onPageChange(event: PageEvent): void {

    this.pageNumber.set(event.pageIndex + 1);

    this.pageSize.set(event.pageSize);

    this.loadAuditHistory();

  }

  clearSearch(): void {

    this.search.set('');

    this.pageNumber.set(1);

    this.loadAuditHistory();

  }

  close(): void {

    this.dialogRef.close();

  }

  getFieldChipColor(field: string): string {

    switch (field) {

      case 'FullName':
        return 'primary';

      case 'Email':
        return 'accent';

      case 'BloodGroup':
        return 'warn';

      case 'ContactNumber':
        return 'primary';

      case 'DateOfBirth':
        return 'accent';

      case 'Gender':
        return 'warn';

      case 'Address':
        return 'primary';

      default:
        return 'primary';

    }

  }

  formatFieldName(field: string): string {

    switch (field) {

      case 'FullName':
        return 'Full Name';

      case 'ContactNumber':
        return 'Contact Number';

      case 'BloodGroup':
        return 'Blood Group';

      case 'DateOfBirth':
        return 'Date of Birth';

      default:
        return field;

    }

  }

  getOldValue(value: string | null): string {

    if (!value || value.trim() === '') {
      return '—';
    }

    return value;

  }

  getNewValue(value: string | null): string {

    if (!value || value.trim() === '') {
      return '—';
    }

    return value;

  }

  trackByAuditId(
    index: number,
    item: PatientAuditHistory
  ): string {

    return item.id;

  }

  ngOnDestroy(): void {

    this.searchSubscription?.unsubscribe();

  }
  getFieldChipClass(field: string): string {
    switch (field) {
      case 'Email':
        return 'chip-email';
      case 'BloodGroup':
        return 'chip-blood';
      case 'DateOfBirth':
        return 'chip-dob';
      case 'ContactNumber':
        return 'chip-contact';
      default:
        return 'chip-default';
    }
  }
}