import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { BaseHttpService } from './base-http.service';
import { AdmissionDetails, AdmissionSummary, AdmitPatientPayload, DischargePatientPayload, TransferBedPayload } from '../models/admission.model';
import { PaginationParams } from '../models/manage-room.model';
import { PagedResponse } from '../models/paged-response.model';

@Injectable({
  providedIn: 'root'
})
export class AdmissionService {
  private readonly baseHttp = inject(BaseHttpService);

  private buildParams(p: PaginationParams): HttpParams {
    let params = new HttpParams()
      .set('PageNumber', p.pageNumber)
      .set('PageSize', p.pageSize);
    
    if (p.search) params = params.set('Search', p.search);
    
    return params;
  }

  getAdmissions(params: PaginationParams) {
    let httpParams = new HttpParams()
      .set('PageNumber', params.pageNumber)
      .set('PageSize', params.pageSize);
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.status !== undefined && params.status !== null) {
      httpParams = httpParams.set('status', params.status);
    }
    return this.baseHttp.get<PagedResponse<AdmissionSummary>>(`admissions`, { params: httpParams });
  }

  admitPatient(payload: AdmitPatientPayload) {
    return this.baseHttp.post<any>(`admissions`, payload, { showSuccess: true });
  }

  transferPatient(admissionId: string, payload: TransferBedPayload) {
    return this.baseHttp.post<any>(`admissions/${admissionId}/transfer`, payload, { showSuccess: true });
  }

  dischargePatient(admissionId: string, payload: DischargePatientPayload) {
    return this.baseHttp.post<any>(`admissions/${admissionId}/discharge`, payload, { showSuccess: true });
  }

  getAdmissionById(admissionId: string) {
    return this.baseHttp.get<AdmissionDetails>(`admissions/${admissionId}`);
  }
  
  updateAdmission(admissionId: string, payload: AdmitPatientPayload) {
    return this.baseHttp.put<any>(`admissions/${admissionId}`, payload, { showSuccess: true });
  }

  getActivePatients(roomId: string) {
    return this.baseHttp.get<any[]>(`admissions/${roomId}/active-patients`);
  }
}