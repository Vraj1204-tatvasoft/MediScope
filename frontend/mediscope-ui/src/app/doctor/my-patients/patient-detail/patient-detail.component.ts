import { Component, OnInit, signal, computed, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

// Angular Material Components
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';

// Core Business Services & Shared Reusable Modules
import { NotificationService } from '../../../core/services/notification.service';
import { DoctorPatientService } from '../../../services/doctor-patient.service';
import { DoctorPatientResponseDto } from '../../../models/doctor-patient,model';
import { HealthHistoryComponent } from '../../../patient/health-history/health-history.component';
import { AddHealthDataComponent } from '../../../patient/add-health-data/add-health-data.component';
import { TrendChartsComponent } from './trend-charts/trend-charts.component';
import { PatientDocumentsTabComponent } from './patient-documents-tab/patient-documents-tab.component';

@Component({
  selector: 'app-patient-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    HealthHistoryComponent,
    AddHealthDataComponent,
    TrendChartsComponent,
    PatientDocumentsTabComponent
  ],
  templateUrl: './patient-detail.component.html',
  styleUrls: ['./patient-detail.component.css']
})
export class PatientDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private dpService = inject(DoctorPatientService);
  private notify = inject(NotificationService);
  @ViewChild(HealthHistoryComponent) historyTab!: HealthHistoryComponent;
  // STATE DEFINITIONS
  patientId = '';
  isLoading = signal<boolean>(true);
  patientProfile = signal<DoctorPatientResponseDto | null>(null);
  selectedTabIndex = 0;
  // ── ADDED: DYNAMIC AGE COMPUTED SIGNAL ──────────────────────────────
  patientAge = computed<string>(() => {
    const profile = this.patientProfile();
    // Adjust 'dateOfBirth' if your DTO uses a different property name like 'dob'
    if (!profile || !profile.dateOfBirth) {
      return '—';
    }

    const birthDate = new Date(profile.dateOfBirth);
    const today = new Date();
    
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDifference = today.getMonth() - birthDate.getMonth();
    
    // Adjust age if the patient's birthday hasn't occurred yet this current year
    if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    
    return age >= 0 ? `${age} yrs` : '—';
  });
  // ─────────────────────────────────────
  // INITIALIZATION ENGINE
  // ─────────────────────────────────────
  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id') || '';
    if (this.patientId) {
      this.loadPatientDeepContext();
    }

    this.route.queryParams.subscribe(params => {
      if (params['tab'] === 'documents') {
        this.selectedTabIndex = 3; 
      } else if (params['tab'] === 'charts') {
        this.selectedTabIndex = 2;
      } else if (params['tab'] === 'add-data') {
        this.selectedTabIndex = 1; 
      }
    });
  }

  // ─────────────────────────────────────
  // DATA MANAGEMENT METHODS
  // ─────────────────────────────────────
  onHealthRecordSaved(): void {
    this.selectedTabIndex = 0;
    if (this.historyTab) {
      this.historyTab.ngOnInit(); 
    }
  }
  loadPatientDeepContext(): void {
    this.isLoading.set(true);

    this.dpService.getMyPatients().subscribe({
      next: (list) => {
        const profile = list.find(p => p.patientId === this.patientId);

        if (profile) {
          this.patientProfile.set(profile);
          this.isLoading.set(false); 
        } else {
          this.isLoading.set(false);
          this.notify.error('Requested patient profile was not found within your directory permissions.');
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  getInitial(name: string | undefined): string {
    return name ? name.charAt(0).toUpperCase() : 'P';
  }
}