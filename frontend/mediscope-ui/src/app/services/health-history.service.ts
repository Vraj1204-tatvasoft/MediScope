import { Injectable } from '@angular/core';
import { BaseHttpService } from './base-http.service';
import { map } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';
import { PagedResponse } from '../models/paged-response.model';
import { HealthMetricSubmission } from '../models/health-history.model';

@Injectable({
  providedIn: 'root'
})
export class HealthHistoryService {
  constructor(
    private baseHttp: BaseHttpService
  ) {}
  getHistoryByPatientId(
    patientId: string,
    pageNumber: number = 1,
    pageSize: number = 7,
    search?: string,
    status?: string,
    source?: string,
    sortBy?: string,
    sortDir?: 'asc' | 'desc'
  ) {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    if (status && status !== 'ALL') {
      params = params.set('status', status);
    }
    if (source && source !== 'ALL') {
      params = params.set('source', source);
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }
    if (sortDir) {
      params = params.set('sortDir', sortDir);
    }
    return this.baseHttp
      .get<PagedResponse<HealthMetricSubmission>>(
        `health-metrics/patient/${patientId}`,
        { params }
      )
      .pipe(map(res => res.data));
  }

  getMyMetrics(
    pageNumber: number = 1,
    pageSize: number = 7,
    search?: string,
    status?: string,
    source?: string,
    sortBy?: string,
    sortDir?: 'asc' | 'desc'
  ) {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    if (status && status !== 'ALL') {
      params = params.set('status', status);
    }
    if (source && source !== 'ALL') {
      params = params.set('source', source);
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }
    if (sortDir) {
      params = params.set('sortDir', sortDir);
    }
    return this.baseHttp
      .get<PagedResponse<HealthMetricSubmission>>(
        'health-metrics/me/paged',
        { params }
      )
      .pipe(map(res => res.data));
  }

  deleteSubmission(submissionId: string) {
    return this.baseHttp.delete<boolean>(`health-metrics/${submissionId}`, {
      showError: true,
      showSuccess: true
    });
  }
}