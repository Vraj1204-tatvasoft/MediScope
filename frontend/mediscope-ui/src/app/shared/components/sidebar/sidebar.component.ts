import { Component, computed, inject, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/auth.model';
import { NAV_CONFIG } from '../../../core/constants/nav.config';
import { DoctorPatientService } from '../../../services/doctor-patient.service'; 
import { PatientNotificationService } from '../../../services/patient-notification.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent implements OnInit {
  authService = inject(AuthService);
  private notificationService = inject(PatientNotificationService);
  private doctorPatientService = inject(DoctorPatientService);
  expandedMenu = '';
  user   = computed(() => this.authService.currentUser());
  config = computed(() => NAV_CONFIG[this.user()?.role as UserRole] ?? NAV_CONFIG['Patient']);

  notificationCount = computed(() => this.notificationService.unreadCount());

  pendingRequestsCount = computed(() => {
    if (this.user()?.role?.toUpperCase() !== 'DOCTOR') {
      return 0;
    }
    return this.doctorPatientService.pendingRequestsCount?.() ?? 0;
  });

  userInitial = computed(() => {
    const name = this.user()?.fullName ?? '';
    return name.charAt(0).toUpperCase();
  });

  toggleMenu(label: string) {
    this.expandedMenu =
      this.expandedMenu === label ? '' : label;
  }
  
  ngOnInit(): void {
    if (this.user()) {
      this.notificationService.syncUnreadCount();
      
      if (this.user()?.role?.toUpperCase() === 'DOCTOR') {
        this.doctorPatientService.pendingRequestsCount?.(); 
      }
    }
  }
}