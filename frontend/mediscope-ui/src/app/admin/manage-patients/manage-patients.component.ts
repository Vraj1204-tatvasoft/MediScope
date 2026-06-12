import { Component, OnInit, signal, computed, inject, OnDestroy } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs'; 
import { debounceTime, distinctUntilChanged } from 'rxjs/operators'; 
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdminPatientOverviewContainer, AdminPatientRowItem } from '../../models/manage-patients.model';
import { ManagePatientsService } from '../../services/manage-patients.service';

@Component({
  selector: 'app-manage-patients',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './manage-patients.component.html',
  styleUrls: ['./manage-patients.component.css']
})
export class ManagePatientsComponent implements OnInit, OnDestroy { 
  private patientService = inject(ManagePatientsService);

  // Layout View State Handlers
  isLoading = signal<boolean>(true);
  dashboardData = signal<AdminPatientOverviewContainer | null>(null);

  // Filter & Pagination Query Nodes
  searchQuery = signal<string>('');
  selectedGender = signal<string>('ALL');
  currentPage = signal<number>(1);
  readonly pageSize = 7;

  // ── NEW: DEBOUNCE STREAM PIPELINE
  private searchSubject = new Subject<string>();
  private searchSubscription!: Subscription;

  // Derived Functional State Calculations
  patientsList = computed<AdminPatientRowItem[]>(() => {
    return this.dashboardData()?.patients.items || [];
  });

  totalRecordsCount = computed<number>(() => {
    return this.dashboardData()?.patients.totalCount || 0;
  });

  totalPagesCount = computed<number>(() => {
    return this.dashboardData()?.patients.totalPages || 1;
  });

  ngOnInit(): void {
    this.loadPatientsDatasetFeed();

    //  INITIALIZE DEBOUNCE LISTENERS
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300), // Wait for 300ms of no typing before hitting the backend
      distinctUntilChanged() // Only fire if the text actually changed
    ).subscribe(query => {
      this.searchQuery.set(query);
      this.currentPage.set(1); // Reset back to page 1
      this.loadPatientsDatasetFeed();
    });
  }

  loadPatientsDatasetFeed(): void {
    this.isLoading.set(true);
    
    this.patientService.getAdminPatients(
      this.currentPage(),
      this.pageSize,
      this.searchQuery(),
      this.selectedGender()
    ).subscribe({
      next: (res) => {
        if (res && res.success) {
          this.dashboardData.set(res.data);
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ── UPDATED EVENT HANDLERS
  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject.next(value); // Push keystroke value into the debounce stream
  }

  onGenderFilterChange(): void {
    this.currentPage.set(1);
    this.loadPatientsDatasetFeed();
  }

  onPageChange(targetPage: number): void {
    if (targetPage >= 1 && targetPage <= this.totalPagesCount()) {
      this.currentPage.set(targetPage);
      this.loadPatientsDatasetFeed();
    }
  }

  getInitial(name: string | undefined): string {
    return name ? name.charAt(0).toUpperCase() : 'P';
  }

  getDoctorsList(doctors: string[]): string {
    if (!doctors || doctors.length === 0 || doctors[0] === '—') return '—';
    return doctors.join(', ');
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }
}