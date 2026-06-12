import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './patient/register/register.component';
import { LayoutComponent } from './shared/components/layout/layout.component';

import { AdminDashboardComponent } from './admin/admin-dashboard/admin-dashboard.component';
import { ManageDoctorsComponent } from './admin/manage-doctors/manage-doctors.component';
import { ManageMetricsComponent } from './admin/manage-metrics/manage-metrics.component';

import { DoctorDashboardComponent } from './doctor/doctor-dashboard/doctor-dashboard.component';
import { MyPatientsComponent } from './doctor/my-patients/my-patients.component';
import { PatientDetailComponent } from './doctor/my-patients/patient-detail/patient-detail.component';
import { DoctorNotificationsComponent } from './doctor/doctor-notifications/doctor-notifications.component';
import { PendingRequestsComponent } from './doctor/pending-requests/pending-requests.component';
import { DoctorProfileComponent } from './doctor/doctor-profile/doctor-profile.component';

import { PatientDashboardComponent } from './patient/patient-dashboard/patient-dashboard.component';
import { AddHealthDataComponent } from './patient/add-health-data/add-health-data.component';
import { HealthHistoryComponent } from './patient/health-history/health-history.component';
import { MyDoctorsComponent } from './patient/my-doctors/my-doctors.component';
import { PatientProfileComponent } from './patient/patient-profile/patient-profile.component';
import { PatientNotificationComponent } from './patient/patient-notification/patient-notification.component';
import { forcePasswordChangeGuard } from './core/guards/force-password-change.guard';
import { ManagePatientsComponent } from './admin/manage-patients/manage-patients.component';
import { DoctorPatientLinksComponent } from './admin/doctor-patient-links/doctor-patient-links.component';
import { HealthRecordDetailComponent } from './patient/health-record-detail/health-record-detail.component';
import { ForgotPasswordComponent } from './auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './auth/reset-password/reset-password.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'register',
    component: RegisterComponent,
  },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password',  component: ResetPasswordComponent  },
  
  // ── PATIENT SECURITY BOUNDARY ─────────────────────────────────
  {
    path: 'patient',
    component: LayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Patient'] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: PatientDashboardComponent,
      },
      {
        path: 'add-health-data',
        component: AddHealthDataComponent,
      },
      {
        path: 'health-history',
        component: HealthHistoryComponent,
      },
      {
        path: 'health-history/:id',
        component: HealthRecordDetailComponent,
      },
      {
        path: 'my-doctors',
        component: MyDoctorsComponent,
      },
      {
        path: 'patient-notifications',
        component: PatientNotificationComponent
      },
      {
        path: 'profile',
        component: PatientProfileComponent,
      },
    ],
  },

  // ── DOCTOR SECURITY BOUNDARY ──────────────────────────────────
  {
    path: 'doctor',
    component: LayoutComponent,
    canActivate: [authGuard, forcePasswordChangeGuard, roleGuard],
    data: { roles: ['Doctor'] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: DoctorDashboardComponent,
      },
      {
        path: 'my-patients',
        component: MyPatientsComponent,
      },
      {
        path: 'my-patients/:id',
        component: PatientDetailComponent,
      },
      {
        path: 'doctor-notifications',
        component: DoctorNotificationsComponent,
      },
      {
        path: 'pending-requests',
        component: PendingRequestsComponent,
      },
      {
        path: 'profile',
        component: DoctorProfileComponent,
      },
    ],
  },

  // ── ADMIN SECURITY BOUNDARY ───────────────────────────────────
  {
    path: 'admin',
    component: LayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: AdminDashboardComponent,
      },
      {
        path: 'manage-doctors',
        component: ManageDoctorsComponent,
      },
      {
        path: 'manage-patients',
        component: ManagePatientsComponent,
      },
      {
        path: 'manage-metrics',
        component: ManageMetricsComponent,
      },
      {
        path: 'doctor-patient-links',
        component: DoctorPatientLinksComponent,
      },
    ],
  },

  // Wildcard fallback path tracking redirection links
  {
    path: '**',
    redirectTo: 'login',
  },
];