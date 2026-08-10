import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service';
import { ApiResponse } from '../models/api-response.model';
import {
  BroadcastPagedResult,
  BroadcastDetail,
  CreateBroadcastRequestDto,
  UpdateBroadcastRequestDto,
  GetBroadcastsRequestDto,
  BroadcastSendResponse,
  BroadcastRetryResponse,
  AudienceCountResponse,
} from '../models/admin-broadcast.model';

@Injectable({ providedIn: 'root' })
export class BroadcastService {
  private readonly http     = inject(BaseHttpService);
  private readonly endpoint = 'broadcasts';

  getBroadcasts(request: GetBroadcastsRequestDto): Observable<ApiResponse<BroadcastPagedResult>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber)
      .set('pageSize',   request.pageSize);

    if (request.search) params = params.set('search', request.search);

    return this.http.get<BroadcastPagedResult>(this.endpoint, { params });
  }

  getBroadcastById(id: string): Observable<ApiResponse<BroadcastDetail>> {
    return this.http.get<BroadcastDetail>(`${this.endpoint}/${id}`);
  }

  // audience = integer (0–3) matching C# BroadcastAudience enum
  getAudienceCount(audience: number): Observable<ApiResponse<AudienceCountResponse>> {
    const params = new HttpParams().set('audience', audience);
    return this.http.get<AudienceCountResponse>(
      `${this.endpoint}/audience-count`, { params }
    );
  }

  createBroadcast(data: CreateBroadcastRequestDto): Observable<ApiResponse<{ id: string }>> {
    return this.http.post<{ id: string }>(this.endpoint, data, {
      showSuccess: true, showError: true,
    });
  }

  updateBroadcast(id: string, data: UpdateBroadcastRequestDto): Observable<ApiResponse<null>> {
    return this.http.put<null>(`${this.endpoint}/${id}`, data, {
      showSuccess: true, showError: true,
    });
  }

  deleteBroadcast(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`${this.endpoint}/${id}`, {
      showSuccess: true, showError: true,
    });
  }

  sendBroadcast(id: string): Observable<ApiResponse<BroadcastSendResponse>> {
    return this.http.post<BroadcastSendResponse>(
      `${this.endpoint}/${id}/send`, {},
      { showSuccess: true, showError: true }
    );
  }

  retryBroadcast(id: string): Observable<ApiResponse<BroadcastRetryResponse>> {
    return this.http.post<BroadcastRetryResponse>(
      `${this.endpoint}/${id}/retry`, {},
      { showSuccess: true, showError: true }
    );
  }
}