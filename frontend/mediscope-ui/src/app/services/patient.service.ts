// FILE: src/app/core/services/patient.service.ts

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { BaseHttpService } from './base-http.service';
import {
  PatientProfile,
  UpdateProfileRequest,
  ChangePasswordRequest
} from '../models/patient.model';

@Injectable({ providedIn: 'root' })
export class PatientService {

  private http = inject(BaseHttpService);

  getMyProfile(): Observable<PatientProfile> {
    return this.http
      .get<PatientProfile>('patient/profile', {
        showError: true,
      })
      .pipe(map(r => r.data));
  }

  updateProfile(request: UpdateProfileRequest): Observable<PatientProfile> {
    return this.http
      .put<PatientProfile>('patient/profile', request, {
        showSuccess: true,   
        showError:   true,
      })
      .pipe(map(r => r.data));
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http
      .patch<void>('patient/change-password', request, {
        showSuccess: true,
        showError:   true,
      })
      .pipe(map(() => void 0));
  }
}