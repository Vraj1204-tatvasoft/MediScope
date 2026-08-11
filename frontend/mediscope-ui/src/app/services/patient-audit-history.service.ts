import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BaseHttpService } from './base-http.service';
import { ApiResponse } from '../models/api-response.model';
import { PatientAuditHistoryResponse } from '../models/patient-audit-history.model';

@Injectable({
  providedIn: 'root'
})
export class PatientAuditHistoryService {

  private baseHttp = inject(BaseHttpService);

  getAuditHistory(
    patientId: string,
    page: number,
    pageSize: number,
    search?: string,
    fieldName?: string,
    changedByUserId?: string,
    fromDate?: Date,
    toDate?: Date
  ): Observable<ApiResponse<PatientAuditHistoryResponse>> {

    let params = new HttpParams()
      .set('pageNumber', page)
      .set('pageSize', pageSize);

    if (search?.trim())
      params = params.set('search', search);

    if (fieldName)
      params = params.set('fieldName', fieldName);

    if (changedByUserId)
      params = params.set('changedByUserId', changedByUserId);

    if (fromDate)
      params = params.set('fromDate', fromDate.toISOString());

    if (toDate)
      params = params.set('toDate', toDate.toISOString());

    return this.baseHttp.get<PatientAuditHistoryResponse>(
      `patient/admin/${patientId}/audit-logs`,
      {
        params,
        showError: true,
        showSuccess: false
      });
  }

}