import { Component, OnInit, signal, computed, Input } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HealthHistoryRow, HistorySummaryStats } from '../../models/health-history.model';
import { HealthHistoryService } from '../../services/health-history.service';
import { MatMenuModule } from '@angular/material/menu';
import { MatDivider } from '@angular/material/divider';
import { AddHealthDataComponent } from '../add-health-data/add-health-data.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-health-history',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatDivider
  ],
  templateUrl: './health-history.component.html',
  styleUrls: ['./health-history.component.css']
})
export class HealthHistoryComponent implements OnInit {
  protected readonly Math = Math;
  @Input() patientId?: string;
  isLoading = signal<boolean>(true);
  rawHistoryRows = signal<HealthHistoryRow[]>([]);
  
  searchQuery = signal<string>('');
  selectedStatus = signal<string>('ALL');
  selectedSource = signal<string>('ALL');

  sortColumn = signal<string>('date'); 
  sortDirection = signal<'asc' | 'desc'>('desc'); 

  currentPage = signal<number>(1);
  pageSize = signal<number>(7);
  totalRecords = signal<number>(0);

  // Deletion State Trackers
  deletingId = signal<string | null>(null);
  showDeleteModal = signal<boolean>(false);
  targetDeleteId = signal<string | null>(null);

  serverSummary = signal<HistorySummaryStats>({ totalRecords: 0, normal: 0, elevated: 0, critical: 0 });

  totalPages = computed<number>(() => {
    return Math.ceil(this.totalRecords() / this.pageSize()) || 1;
  });

  summaryStats = computed<HistorySummaryStats>(() => this.serverSummary());

  dynamicColumns = computed<{ main: string[]; drawer: string[] }>(() => {
    const rows = this.rawHistoryRows();
    const uniqueKeys = new Set<string>();
    rows.forEach(row => {
      Object.keys(row.metrics).forEach(key => {
        if (key !== 'systolic_blood_pressure' && key !== 'dialostic_blood_pressure') {
          uniqueKeys.add(key);
        }
      });
    });
    const allKeys = Array.from(uniqueKeys);
    return {
      main: allKeys.slice(0, 4),
      drawer: allKeys.slice(4)
    };
  });

  constructor(private historyService: HealthHistoryService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadPatientHistoryData();
  }

  loadPatientHistoryData(): void {
    this.isLoading.set(true);
    
    const page = this.currentPage();
    const size = this.pageSize();
    const search = this.searchQuery();
    const status = this.selectedStatus();
    const source = this.selectedSource();
    const sortBy = this.sortColumn();
    const sortDir = this.sortDirection();
    
    const dataStream$ = this.patientId
      ? this.historyService.getHistoryByPatientId(this.patientId, page, size, search, status, source, sortBy, sortDir)
      : this.historyService.getMyMetrics(page, size, search, status, source, sortBy, sortDir);

    dataStream$.subscribe({
      next: (response: any) => {
        const itemsList = response.items || response;
        this.rawHistoryRows.set(this.parseSubmissions(itemsList || []));
        this.currentPage.set(response.pageNumber || page);
        this.totalRecords.set(response.totalCount || itemsList.length || 0);
        
        const statsSource = response.summaryStats || (response as any).SummaryStats;
        if (statsSource) {
          this.serverSummary.set({
            totalRecords: statsSource.totalRecords ?? statsSource.TotalRecords ?? 0,
            normal: statsSource.normal ?? statsSource.Normal ?? 0,
            elevated: statsSource.elevated ?? statsSource.Elevated ?? 0,
            critical: statsSource.critical ?? statsSource.Critical ?? 0
          });
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  //  CUSTOM MODAL DELETE ACTIONS ─────────────────────────────────
  openDeleteConfirmation(submissionId: string, event: Event): void {
    event.stopPropagation(); // Prevents table row drawer from toggling
    this.targetDeleteId.set(submissionId);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.targetDeleteId.set(null);
  }

  confirmDeleteRecord(): void {
    const idToDelete = this.targetDeleteId();
    if (!idToDelete) return;

    this.deletingId.set(idToDelete);
    this.showDeleteModal.set(false); // Close modal overlay instantly

    this.historyService.deleteSubmission(idToDelete).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.loadPatientHistoryData();
        }
        this.deletingId.set(null);
        this.targetDeleteId.set(null);
      },
      error: () => {
        this.deletingId.set(null);
        this.targetDeleteId.set(null);
      }
    });
  }

  onPageChange(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages()) {
      this.currentPage.set(newPage);
      this.loadPatientHistoryData();
    }
  }

  onSort(columnName: string): void {
    if (columnName !== 'date' && columnName !== 'addedBy' && columnName !== 'status') return;
    if (this.sortColumn() === columnName) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(columnName);
      this.sortDirection.set('asc');
    }
    this.currentPage.set(1);
    this.loadPatientHistoryData();
  }

  onFilterChange(): void {
    this.currentPage.set(1); 
    this.loadPatientHistoryData();
  }

  formatHeaderLabel(key: string): string {
    if (key === 'blood_pressure') return 'BP';
    if (key === 'heart_rate') return 'HR';
    return key.replace(/_/g, ' ').toUpperCase();
  }

  editRecord(row: any) {
    const dialogRef = this.dialog.open(AddHealthDataComponent, {
      width: '800px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      data: row, // Pass the row data here!
      disableClose: true
    });
  
    dialogRef.afterClosed().subscribe((success:any) => {
      if (success) {
        this.loadPatientHistoryData(); 
      }
    });
  }

  toggleRow(row: HealthHistoryRow): void {
    row.isExpanded = !row.isExpanded;
  }

  private parseSubmissions(items: any[]): HealthHistoryRow[] {
    return items.map(submission => {
      const rowMetrics: HealthHistoryRow['metrics'] = {};
      const flaggedMetrics: string[] = [];
      let systolicVal: string | null = null;
      let diastolicVal: string | null = null;
      let bpIsAbnormal = false;

      submission.metrics.forEach((item: any) => {
        const typeKey = item.metricType ? item.metricType.toLowerCase().trim() : '';
        const status = item.status ? item.status.toUpperCase() : 'NORMAL';
        const isAbnormalItem = status === 'HIGH' || status === 'LOW' || status === 'ELEVATED' || status === 'CRITICAL';

        if (typeKey === 'systolic_blood_pressure') {
          systolicVal = Math.round(item.value).toString();
          if (isAbnormalItem) bpIsAbnormal = true;
        }
        if (typeKey === 'dialostic_blood_pressure') {
          diastolicVal = Math.round(item.value).toString();
          if (isAbnormalItem) bpIsAbnormal = true;
        }

        if (typeKey !== '' && typeKey !== 'systolic_blood_pressure' && typeKey !== 'dialostic_blood_pressure') {
          let formattedValue = `${item.value} ${item.unit || ''}`;
          if (typeKey === 'heart_rate') formattedValue = `${Math.round(item.value)}`;
          if (typeKey === 'sleep') formattedValue = `${item.value} hrs`;

          rowMetrics[typeKey] = { displayValue: formattedValue, rawVal: item.value };
          if (isAbnormalItem) flaggedMetrics.push(typeKey);
        }
      });

      if (systolicVal || diastolicVal) {
        rowMetrics['blood_pressure'] = {
          displayValue: `${systolicVal || '—'}/${diastolicVal || '—'}`,
          rawVal: Number(systolicVal || 0) 
        };
        if (bpIsAbnormal) flaggedMetrics.push('blood_pressure');
      }

      let cleanDisplayDate = '—';
      if (submission.recordedAt) {
        cleanDisplayDate = submission.recordedAt.includes('T')
          ? submission.recordedAt.split('T')[0]
          : submission.recordedAt.substring(0, 10);
      }

      return {
        submissionId: submission.submissionId,
        date: cleanDisplayDate,
        addedBy: submission.recordedByName || 'Patient',
        isDoctor: (submission.recordedByRole || '').toUpperCase() !== 'PATIENT',
        notes: submission.notes,
        status: submission.status === 'NORMAL' ? 'Normal' : submission.status === 'CRITICAL' ? 'Critical' : 'Elevated',
        flaggedMetrics: flaggedMetrics,
        isExpanded: false,
        metrics: rowMetrics
      };
    });
  }

  exportCSV(): void {
    const search = this.searchQuery();
    const status = this.selectedStatus();
    const source = this.selectedSource();
    const sortBy = this.sortColumn();
    const sortDir = this.sortDirection();

    // We pass a large page size (999999) to pull the full historical record set without page breaks
    const dataStream$ = this.patientId
      ? this.historyService.getHistoryByPatientId(this.patientId, 1, 999999, search, status, source, sortBy, sortDir)
      : this.historyService.getMyMetrics(1, 999999, search, status, source, sortBy, sortDir);

    dataStream$.subscribe({
      next: (response) => {
        // Safe check to unpack either a paged data items wrapper or a raw array
        const itemsList = response.items || response;
        const fullRows = this.parseSubmissions(itemsList || []);
        
        // Define consistent layout columns configuration matching your screen view
        const mainKeys = ['heart_rate', 'blood_sugar', 'sleep', 'blood_pressure'];
        const drawerKeys = ['cholestrol', 'temperature', 'o2_saturation'];
        const allMetricKeys = [...mainKeys, ...drawerKeys];

        const baseHeaders = ['Date', 'Added By', 'Status', 'Notes'];
        const metricHeaders = allMetricKeys.map(key => this.formatHeaderLabel(key));
        const finalHeaders = [...baseHeaders, ...metricHeaders];
        
        let csvContent = 'data:text/csv;charset=utf-8,' + finalHeaders.join(',') + '\n';

        fullRows.forEach(row => {
          const addedBy = row.addedBy ? row.addedBy.replace(/"/g, '""') : '';
          const notes = row.notes ? row.notes.replace(/"/g, '""').replace(/\n/g, ' ') : '';

          const lineCells = [`"${row.date}"`, `"${addedBy}"`, `"${row.status}"`, `"${notes}"`];

          allMetricKeys.forEach(metricKey => {
            const cellData = row.metrics[metricKey];
            lineCells.push(`"${cellData ? cellData.displayValue.replace(/"/g, '""') : '—'}"`);
          });

          csvContent += lineCells.join(',') + '\n';
        });

        // Trigger native browser download utility stream
        const encodedUri = encodeURI(csvContent);
        const link = document.createElement('a');
        link.setAttribute('href', encodedUri);
        
        // Custom dynamic filename depending on context
        const fileName = this.patientId ? 'Patient_Health_History_Export.csv' : 'My_Health_History_Export.csv';
        link.setAttribute('download', fileName);
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      },
      error: () => {
        // Fallback safety alert hook
        console.error('CSV Extraction Pipeline Fault.');
      }
    });
  }
}