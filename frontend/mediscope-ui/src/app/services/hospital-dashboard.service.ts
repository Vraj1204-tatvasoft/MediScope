import { HttpParams } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';

import { HospitalDashboardFilter } from '../models/hospital-dashboard-filter.model';
import {
  HospitalDashboardResponse,
  HospitalRoom,
  HospitalSummary
} from '../models/hospital-dashboard.model';
import { BaseHttpService } from './base-http.service';

@Injectable({
  providedIn: 'root'
})
export class HospitalDashboardService {

  private readonly http = inject(BaseHttpService);
  private readonly endpoint = 'hospitalization-dashboard';

  readonly loading = signal(false);

  private readonly dashboardSignal = signal<HospitalDashboardResponse | null>(null);

  readonly filter = signal<HospitalDashboardFilter>({
    pageNumber: 1,
    pageSize: 7,
    sortBy: 'roomnumber',
    sortDir: 'asc'
  });

  readonly summary = computed<HospitalSummary | null>(() =>
    this.dashboardSignal()?.summary ?? null
  );

  readonly rooms = computed<HospitalRoom[]>(() =>
    this.dashboardSignal()?.rooms.items ?? []
  );

  readonly pagination = computed(() =>
    this.dashboardSignal()?.rooms ?? null
  );

  loadDashboard(): void {
    this.loading.set(true);

    let params = new HttpParams();

    Object.entries(this.filter()).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value.toString());
      }
    });

    this.http.get<HospitalDashboardResponse>(this.endpoint, {
      params,
      showError: true
    }).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: response => {
        if (response.success) {
          this.dashboardSignal.set(response.data);
        }
      }
    });
  }

  updateFilter(filter: Partial<HospitalDashboardFilter>): void {
    this.filter.update(current => ({
      ...current,
      ...filter
    }));

    this.loadDashboard();
  }

  changePage(pageNumber: number): void {
    this.updateFilter({ pageNumber });
  }

  changePageSize(pageSize: number): void {
    this.updateFilter({
      pageNumber: 1,
      pageSize
    });
  }

  resetFilters(): void {
    this.filter.set({
      pageNumber: 1,
      pageSize: 7,
      sortBy: 'roomnumber',
      sortDir: 'asc'
    });

    this.loadDashboard();
  }

  refresh(): void {
    this.loadDashboard();
  }
}