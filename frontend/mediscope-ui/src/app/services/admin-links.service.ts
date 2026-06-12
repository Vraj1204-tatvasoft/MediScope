import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminOverviewResponseDto } from '../models/admin-links.model';
import { ApiResponse } from '../models/api-response.model';
import { BaseHttpService } from './base-http.service';

@Injectable({ providedIn: 'root' })
export class AdminLinksService {
  private baseHttp = inject(BaseHttpService);

  getAdminOverview(
    pageNumber: number = 1,
    pageSize: number = 7,
    search?: string, 
    doctorId?: string, 
    status?: string
  ): Observable<ApiResponse<AdminOverviewResponseDto>> {
    let queryParams = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    if (search?.trim()) queryParams = queryParams.set('search', search.trim());
    if (doctorId && doctorId !== 'ALL') queryParams = queryParams.set('doctorId', doctorId);
    if (status && status !== 'ALL') queryParams = queryParams.set('status', status);

    return this.baseHttp.get<AdminOverviewResponseDto>('doctor-patient/admin-overview', {
      params: queryParams,
      showError: true,
      showSuccess: false
    });
  }
}