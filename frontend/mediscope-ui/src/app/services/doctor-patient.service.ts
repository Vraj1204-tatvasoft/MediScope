// File: src/app/services/doctor-patient.service.ts

import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { PatientDoctorResponseDto, SendDoctorRequestDto, AdminDoctorPatientFilterDto, AdminConnectionRequestDto, AdminApproveRequestDto, AdminRejectRequestDto, DoctorPatientResponseDto, RespondToRequestDto } from '../models/doctor-patient,model';
import { BaseHttpService } from './base-http.service';

@Injectable({ providedIn: 'root' })
export class DoctorPatientService {

  private http = inject(BaseHttpService);

  pendingRequestsCount = signal<number>(0);

  // ── PATIENT ────────────────────────────────────────────────

  getMyDoctors(): Observable<PatientDoctorResponseDto[]> {
    return this.http
      .get<PatientDoctorResponseDto[]>('doctor-patient/my-doctors', { showError: true })
      .pipe(map(r => r.data));
  }

  // DoctorId is optional — patient can request without selecting
  sendRequest(request: SendDoctorRequestDto): Observable<PatientDoctorResponseDto> {
    return this.http
      .post<PatientDoctorResponseDto>('doctor-patient/request', request, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(r => r.data));
  }

  revokeAccess(doctorPatientId: string): Observable<void> {
    return this.http
      .patch<void>('doctor-patient/revoke', { doctorPatientId }, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(() => void 0));
  }

  // ── ADMIN ──────────────────────────────────────────────────

  // All requests with optional filters
  getAllRequestsForAdmin(filter?: AdminDoctorPatientFilterDto): Observable<AdminConnectionRequestDto[]> {
    let params = new HttpParams();
    if (filter?.search)   params = params.set('search',   filter.search);
    if (filter?.doctorId) params = params.set('doctorId', filter.doctorId);
    if (filter?.status && filter.status !== 'ALL')
      params = params.set('status', filter.status);

    return this.http
      .get<AdminConnectionRequestDto[]>('doctor-patient/admin/all', {
        params,
        showError: true,
      })
      .pipe(map(r => r.data));
  }

  // Pending admin review only
  getPendingAdminRequests(): Observable<AdminConnectionRequestDto[]> {
    return this.http
      .get<AdminConnectionRequestDto[]>('doctor-patient/admin/pending', { showError: true })
      .pipe(map(r => r.data));
  }

  // Admin approves and assigns a doctor
  approveRequest(request: AdminApproveRequestDto): Observable<PatientDoctorResponseDto> {
    return this.http
      .patch<PatientDoctorResponseDto>('doctor-patient/admin/approve', request, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(r => r.data));
  }

  // Admin rejects a request
  rejectRequest(request: AdminRejectRequestDto): Observable<void> {
    return this.http
      .patch<void>('doctor-patient/admin/reject', request, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(() => void 0));
  }

  // ── DOCTOR ─────────────────────────────────────────────────

  getPendingRequests(): Observable<DoctorPatientResponseDto[]> {
    return this.http
      .get<DoctorPatientResponseDto[]>('doctor-patient/pending', { showError: true })
      .pipe(map(r => r.data));
  }

  respondToRequest(request: RespondToRequestDto): Observable<DoctorPatientResponseDto> {
    return this.http
      .patch<DoctorPatientResponseDto>('doctor-patient/respond', request, {
        showSuccess: false,
        showError:   true,
      })
      .pipe(map(r => r.data));
  }

  getMyPatients(): Observable<DoctorPatientResponseDto[]> {
    return this.http
      .get<DoctorPatientResponseDto[]>('doctor-patient/my-patients', { showError: true })
      .pipe(map(r => r.data));
  }
}