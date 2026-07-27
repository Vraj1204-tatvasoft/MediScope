import { ChangeDetectionStrategy, Component, OnInit, TemplateRef, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HospitalDashboardService } from '../../services/hospital-dashboard.service';
import { HospitalRoom } from '../../models/hospital-dashboard.model';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgClass } from '@angular/common';
import { ManageRoomService } from '../../services/manage-room.service';
import { PaginationParams, RoomType, WardSummary } from '../../models/manage-room.model';
import { SignalrService } from '../../services/signalr.service';
import { FullCalendarModule } from '@fullcalendar/angular';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { RoomCalendarComponent } from '../room-calendar/room-calendar.component';
import { Router } from '@angular/router';
@Component({
  selector: 'app-hospital-dashboard',
  templateUrl: './hospital-dashboard.component.html',
  styleUrl: './hospital-dashboard.component.css',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    FormsModule,
    NgClass,
    FullCalendarModule,
    MatDialogModule,
    RoomCalendarComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HospitalDashboardComponent implements OnInit {
  @ViewChild('roomCalendarDialogTpl') roomCalendarDialogTpl!: TemplateRef<any>;
  constructor(private dialog: MatDialog, private router: Router) {}
  private readonly dashboardService = inject(HospitalDashboardService);
  private readonly manageRoomService = inject(ManageRoomService);
  private readonly signalrService = inject(SignalrService);
  readonly loading = this.dashboardService.loading;
  readonly summary = this.dashboardService.summary;
  readonly rooms = this.dashboardService.rooms;
  readonly pagination = this.dashboardService.pagination;
  selectedDashboardRoom: any = null;
  readonly filter = signal({
    search: '',
    wardId: '',
    roomTypeId: '',
    floor: null as number | null,
    occupancyStatus: null as number | null
  });
  readonly startRecord = computed(() => {
    const page = this.pagination();
  
    if (!page || page.totalCount === 0) {
      return 0;
    }
  
    return (page.pageNumber - 1) * page.pageSize + 1;
  });
  readonly wards = signal<WardSummary[]>([]);
  readonly roomTypes = signal<RoomType[]>([]);
  readonly endRecord = computed(() => {
    const page = this.pagination();
  
    if (!page) {
      return 0;
    }
  
    return Math.min(page.pageNumber * page.pageSize, page.totalCount);
  });

  readonly totalOccupancy = computed(() => {
    const summary = this.summary();

    if (!summary || summary.totalBeds === 0) {
      return 0;
    }

    return Math.round((summary.occupiedBeds / summary.totalBeds) * 100);
  });
  readonly displayedColumns = [
    'ward',
    'roomNumber',
    'roomType',
    'floor',
    'totalBeds',
    'occupiedBeds',
    'availableBeds',
    'status'
  ];
  
  ngOnInit(): void {
    this.loadDropdownData();
    this.dashboardService.loadDashboard();
    this.signalrService.dashboardUpdated$
    .subscribe(() => {
      console.log('Dashboard refresh triggered');
      this.dashboardService.refresh();
    });
  }

  applyFilters(): void {
    const filter = this.filter();
  
    this.dashboardService.updateFilter({
      search: filter.search || undefined,
      wardId: filter.wardId || undefined,
      roomTypeId: filter.roomTypeId || undefined,
      floor: filter.floor ?? undefined,
      occupancyStatus: filter.occupancyStatus ?? undefined,
      pageNumber: 1
    });
  }

  resetFilters(): void {
    this.filter.set({
      search: '',
      wardId: '',
      roomTypeId: '',
      floor: null,
      occupancyStatus: null
    });

    this.dashboardService.resetFilters();
  }

  changePage(page: number): void {
    this.dashboardService.changePage(page);
  }

  changePageSize(pageSize: number): void {
    this.dashboardService.changePageSize(pageSize);
  }

  getOccupancyPercentage(room: HospitalRoom): number {
    if (!room.totalBeds) {
      return 0;
    }

    return Math.round((room.occupiedBeds / room.totalBeds) * 100);
  }
  getAvailabilityPercentage(room: HospitalRoom): number {
    if (!room.totalBeds) {
      return 0;
    }

    return Math.round((room.availableBeds / room.totalBeds) * 100);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Full':
        return 'warn';

      case 'Partially Occupied':
        return 'accent';

      default:
        return 'primary';
    }
  }

  trackByRoom(_: number, room: HospitalRoom): string {
    return room.id;
  }

  updateWard(wardId: string): void {
    this.filter.update(filter => ({
      ...filter,
      wardId
    }));
  }
  goToAdmitPatient() {
    this.router.navigate(['/admin/admissions'], { queryParams: { action: 'admit' } });
  }
  updateRoomType(roomTypeId: string): void {
    this.filter.update(filter => ({
      ...filter,
      roomTypeId
    }));
  }
  
  updateFloor(floor: number | null): void {
    this.filter.update(filter => ({
      ...filter,
      floor
    }));
  }
  
  updateOccupancyStatus(occupancyStatus: number | null): void {
    this.filter.update(filter => ({
      ...filter,
      occupancyStatus
    }));
  }
  private loadDropdownData(): void {
    const params : PaginationParams = {
      pageNumber: 1,
      pageSize: 1000,
      sortBy: 'name',
      sortDir: 'asc'
    };
  
    this.manageRoomService.getWards(params).subscribe({
      next: response => this.wards.set(response.data.items)
    });
  
    this.manageRoomService.getRoomTypes(params).subscribe({
      next: response => this.roomTypes.set(response.data. items)
    });
  }
  openRoomCalendar(room: any) {
    this.selectedDashboardRoom = room; 
    this.dialog.open(this.roomCalendarDialogTpl, {
      width: '850px',       
      maxWidth: '95vw',
      disableClose: false,  
      autoFocus: false      
    });
  }
}