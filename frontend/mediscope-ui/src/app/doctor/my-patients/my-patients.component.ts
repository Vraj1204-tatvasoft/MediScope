import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { DoctorPatientService } from '../../services/doctor-patient.service';
import { NotificationService } from '../../core/services/notification.service';
import { DoctorPatientResponseDto } from '../../models/doctor-patient,model';

@Component({
  selector: 'app-my-patients',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './my-patients.component.html',
  styleUrls: ['./my-patients.component.css']
})
export class MyPatientsComponent implements OnInit {
  private dpService = inject(DoctorPatientService);
  private notify = inject(NotificationService);
  private router = inject(Router);

  // States
  patients = signal<DoctorPatientResponseDto[]>([]);
  isLoading = signal<boolean>(true);
  searchQuery = signal<string>('');
  selectedStatus = signal<string>('ALL');

  // Directory Analytical Count Metrics
  totalCount = computed(() => this.patients().length);
  
  // Computed filter evaluation pipeline
  filteredPatients = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    return this.patients().filter(p => {
      const matchesSearch = !query || p.fullName.toLowerCase().includes(query) || p.email.toLowerCase().includes(query);
      return matchesSearch;
    });
  });

  ngOnInit(): void {
    this.loadAssignedPatients();
  }

  loadAssignedPatients(): void {
    this.isLoading.set(true);
    this.dpService.getMyPatients().subscribe({
      next: (data) => {
        this.patients.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.notify.error('Failed to pull assigned patient directory index.');
      }
    });
  }

  viewPatientDetails(patientId: string): void {
    this.router.navigate(['/doctor/my-patients', patientId]);
  }

  getInitial(name: string): string {
    return name ? name.charAt(0).toUpperCase() : 'P';
  }
}