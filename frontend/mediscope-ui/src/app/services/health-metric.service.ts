import { Injectable } from '@angular/core';
import { BaseHttpService } from './base-http.service';
import { map } from 'rxjs/operators';
import { AddHealthMetricRequestDto } from '../models/health-metric.model';
import { MetricDefinition } from '../models/metric-definition.model';

@Injectable({ providedIn: 'root' })
export class HealthMetricService {
  constructor(private baseHttp: BaseHttpService) {}

  getMetricDefinitions() {
    return this.baseHttp.get<MetricDefinition[]>('metric-definitions').pipe(
      map(response => response.data)
    );
  }

  saveHealthRecord(request: AddHealthMetricRequestDto, p0: { showSuccess: boolean; }) {
    return this.baseHttp.post<any>('health-metrics', request);
  }
}