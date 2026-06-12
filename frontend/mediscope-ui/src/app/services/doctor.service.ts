// FILE: src/app/core/services/doctor.service.ts

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { BaseHttpService } from './base-http.service';
import {
  DoctorProfile,
  CreateDoctorRequest,
  UpdateDoctorRequest
} from '../models/doctor.model';

@Injectable({ providedIn: 'root' })
export class DoctorService {

  private http = inject(BaseHttpService);

  // ── POST /api/doctor — Admin creates doctor ───────────────
  createDoctor(request: CreateDoctorRequest): Observable<DoctorProfile> {
    return this.http
      .post<DoctorProfile>('doctor', request, {
        showSuccess: true,   // shows "Doctor created successfully."
        showError:   true,
      })
      .pipe(map(r => r.data));
  }

  // ── GET /api/doctor/all — Admin + Patient ─────────────────
  getAllDoctors(): Observable<DoctorProfile[]> {
    return this.http
      .get<DoctorProfile[]>('doctor/all', {
        showError: true,
      })
      .pipe(map(r => r.data));
  }

  // ── GET /api/doctor/{id} — Admin + Patient ────────────────
  getDoctorById(doctorId: string): Observable<DoctorProfile> {
    return this.http
      .get<DoctorProfile>(`doctor/${doctorId}`, {
        showError: true,
      })
      .pipe(map(r => r.data));
  }

  // ── GET /api/doctor/me — Doctor only ─────────────────────
  getMyProfile(): Observable<DoctorProfile> {
    return this.http
      .get<DoctorProfile>('doctor/me', {
        showError: true,
      })
      .pipe(map(r => r.data));
  }

  // ── PUT /api/doctor — Doctor updates own profile ──────────
  updateMyProfile(request: UpdateDoctorRequest): Observable<DoctorProfile> {
    return this.http
      .put<DoctorProfile>('doctor', request, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(r => r.data));
  }

  // ── PATCH /api/doctor/change-password — Doctor only ───────
  changePassword(request: {
    currentPassword:    string;
    newPassword:        string;
    confirmNewPassword: string;
  }): Observable<void> {
    return this.http
      .patch<void>('doctor/change-password', request, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(() => void 0));
  }
}