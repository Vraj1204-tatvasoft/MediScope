import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import {
  MetricDefinition,
  CreateMetricDefinitionRequest,
  UpdateMetricDefinitionRequest
} from '../models/metric-definition.model';
import { BaseHttpService } from './base-http.service';

@Injectable({
  providedIn: 'root'
})
export class MetricDefinitionService {

  private endpoint = 'metric-definitions';

  constructor(
    private http: BaseHttpService
  ) {}

  getAll(): Observable<MetricDefinition[]> {
    return this.http
      .get<MetricDefinition[]>(this.endpoint)
      .pipe(map(res => res.data));
  }

  getById(id: string): Observable<MetricDefinition> {
    return this.http
      .get<MetricDefinition>(`${this.endpoint}/${id}`)
      .pipe(map(res => res.data));
  }

  create(
    request: CreateMetricDefinitionRequest
  ): Observable<MetricDefinition> {

    return this.http
      .post<MetricDefinition>(
        this.endpoint,
        request
      )
      .pipe(map(res => res.data));
  }

  update(
    id: string,
    request: UpdateMetricDefinitionRequest
  ): Observable<MetricDefinition> {

    return this.http
      .put<MetricDefinition>(
        `${this.endpoint}/${id}`,
        request
      )
      .pipe(map(res => res.data));
  }

  toggleStatus(id: string): Observable<MetricDefinition> {
    return this.http
      .patch<MetricDefinition>(`${this.endpoint}/${id}/toggle-status`, {})
      .pipe(map(res => res.data));
  }
}