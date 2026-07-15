import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabChangeEvent, MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort'; 
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field'; 
import { MatInputModule } from '@angular/material/input'; 
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { forkJoin } from 'rxjs'; 
import { BedSummary, PaginationParams, RoomSummary, RoomType, WardSummary } from '../../models/manage-room.model';
import { ManageRoomService } from '../../services/manage-room.service';
import { DynamicDialogComponent, DynamicDialogData } from '../dynamic-dialog/dynamic-dialog.component';

@Component({
  selector: 'app-manage-room',
  standalone: true,
  imports: [
    CommonModule, MatTabsModule, MatTableModule, MatButtonModule, 
    MatIconModule, MatProgressSpinnerModule, MatChipsModule, 
    MatCardModule, MatDialogModule, MatMenuModule,
    MatPaginatorModule, MatSortModule, MatFormFieldModule, MatInputModule, MatProgressBarModule
  ],
  templateUrl: './manage-rooms.component.html',
  styleUrls: ['./manage-rooms.component.css']
})
export class ManageRoomsComponent implements OnInit {
  private readonly facilityService = inject(ManageRoomService);
  private readonly dialog = inject(MatDialog);

  // Data Signals
  wards = signal<WardSummary[]>([]);
  rooms = signal<RoomSummary[]>([]);
  roomTypes = signal<RoomType[]>([]);
  beds = signal<BedSummary[]>([]);
  isLoading = signal<boolean>(false);

  // Total Count Signals (for Paginator)
  totalWards = signal<number>(0);
  totalRooms = signal<number>(0);
  totalRoomTypes = signal<number>(0);
  totalBeds = signal<number>(0);
  selectedTabIndex = 0;
  // Pagination Params State
  wardParams: PaginationParams = { pageNumber: 1, pageSize: 10, sortBy: 'name', sortDir: 'asc' };
  roomParams: PaginationParams = { pageNumber: 1, pageSize: 10, sortBy: 'roomnumber', sortDir: 'asc' };
  typeParams: PaginationParams = { pageNumber: 1, pageSize: 10, sortBy: 'name', sortDir: 'asc' };
  bedParams: PaginationParams = { pageNumber: 1, pageSize: 10, sortBy: 'bednumber', sortDir: 'asc' };

  wardColumns: string[] = ['name', 'description', 'actions'];
  roomColumns: string[] = ['roomNumber', 'wardName', 'roomTypeName', 'bedCount', 'actions'];
  bedColumns: string[] = ['bedNumber', 'roomNumber', 'wardName', 'status', 'actions'];
  typeColumns: string[] = ['name', 'actions'];

  ngOnInit(): void {
    this.loadRooms();
  }

  onTabChange(event: MatTabChangeEvent) {
    this.selectedTabIndex = event.index;
    
    switch(event.index) {
      case 0: this.loadRooms(); break;
      case 1: this.loadBeds(); break;
      case 2: this.loadWards(); break;
      case 3: this.loadRoomTypes(); break;
    }
  }
  
  loadAllData(): void {
    this.isLoading.set(true);
    // ForkJoin allows us to wait until ALL initial grid loads finish to hide the spinner
    forkJoin({
      w: this.facilityService.getWards(this.wardParams),
      r: this.facilityService.getRooms(this.roomParams),
      t: this.facilityService.getRoomTypes(this.typeParams),
      b: this.facilityService.getBeds(this.bedParams)
    }).subscribe(res => {
      if(res.w.success) { this.wards.set(res.w.data.items); this.totalWards.set(res.w.data.totalCount); }
      if(res.r.success) { this.rooms.set(res.r.data.items); this.totalRooms.set(res.r.data.totalCount); }
      if(res.t.success) { this.roomTypes.set(res.t.data.items); this.totalRoomTypes.set(res.t.data.totalCount); }
      if(res.b.success) { this.beds.set(res.b.data.items); this.totalBeds.set(res.b.data.totalCount); }
      this.isLoading.set(false);
    });
  }

  // INDIVIDUAL LOADERS & EVENT HANDLERS

  loadWards() {
    this.isLoading.set(true);
    this.facilityService.getWards(this.wardParams).subscribe({
      next: (res) => {
        if(res.success) { this.wards.set(res.data.items); this.totalWards.set(res.data.totalCount); }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onWardPage(e: PageEvent) { this.wardParams.pageNumber = e.pageIndex + 1; this.wardParams.pageSize = e.pageSize; this.loadWards(); }
  onWardSort(s: Sort) { this.wardParams.sortBy = s.active; this.wardParams.sortDir = s.direction as any; this.loadWards(); }
  onWardSearch(q: string) { this.wardParams.search = q; this.wardParams.pageNumber = 1; this.loadWards(); }

  loadRooms() {
    this.isLoading.set(true);
    this.facilityService.getRooms(this.roomParams).subscribe({
      next: (res) => {
        if(res.success) { this.rooms.set(res.data.items); this.totalRooms.set(res.data.totalCount); }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
  onRoomPage(e: PageEvent) { this.roomParams.pageNumber = e.pageIndex + 1; this.roomParams.pageSize = e.pageSize; this.loadRooms(); }
  onRoomSort(s: Sort) { this.roomParams.sortBy = s.active; this.roomParams.sortDir = s.direction as any; this.loadRooms(); }
  onRoomSearch(q: string) { this.roomParams.search = q; this.roomParams.pageNumber = 1; this.loadRooms(); }

  loadRoomTypes() {
    this.isLoading.set(true);
    this.facilityService.getRoomTypes(this.typeParams).subscribe({
      next: (res) => {
        if(res.success) { this.roomTypes.set(res.data.items); this.totalRoomTypes.set(res.data.totalCount); }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
  onTypePage(e: PageEvent) { this.typeParams.pageNumber = e.pageIndex + 1; this.typeParams.pageSize = e.pageSize; this.loadRoomTypes(); }
  onTypeSort(s: Sort) { this.typeParams.sortBy = s.active; this.typeParams.sortDir = s.direction as any; this.loadRoomTypes(); }
  onTypeSearch(q: string) { this.typeParams.search = q; this.typeParams.pageNumber = 1; this.loadRoomTypes(); }

  loadBeds() {
    this.isLoading.set(true);
    this.facilityService.getBeds(this.bedParams).subscribe({
      next: (res) => {
        if(res.success) { this.beds.set(res.data.items); this.totalBeds.set(res.data.totalCount); }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
  onBedPage(e: PageEvent) { this.bedParams.pageNumber = e.pageIndex + 1; this.bedParams.pageSize = e.pageSize; this.loadBeds(); }
  onBedSort(s: Sort) { this.bedParams.sortBy = s.active; this.bedParams.sortDir = s.direction as any; this.loadBeds(); }
  onBedSearch(q: string) { this.bedParams.search = q; this.bedParams.pageNumber = 1; this.loadBeds(); }


  // DIALOG TRIGGERS (ADD & EDIT)

  openWardDialog(ward?: WardSummary) {
    const data: DynamicDialogData = {
      title: ward?.id ? 'Edit Ward' : 'Add New Ward',
      fields: [
        { key: 'name', label: 'Ward Name', type: 'text', value: ward?.name, required: true },
        { key: 'description', label: 'Description', type: 'textarea', value: ward?.description }
      ]
    };
    this.dialog.open(DynamicDialogComponent, { width: '400px', data }).afterClosed().subscribe(res => {
      if (res) {
        if (ward?.id) this.facilityService.updateWard(ward.id, res).subscribe(() => this.loadWards());
        else this.facilityService.createWard(res).subscribe(() => this.loadWards());
      }
    });
  }

  openRoomTypeDialog(type?: RoomType) {
    const data: DynamicDialogData = {
      title: type?.id ? 'Edit Room Type' : 'Add Room Type',
      fields: [ { key: 'name', label: 'Classification Name', type: 'text', value: type?.name, required: true } ]
    };
    this.dialog.open(DynamicDialogComponent, { width: '400px', data }).afterClosed().subscribe(res => {
      if (res) {
        if (type?.id) this.facilityService.updateRoomType(type.id, res).subscribe(() => this.loadRoomTypes());
        else this.facilityService.createRoomType(res).subscribe(() => this.loadRoomTypes());
      }
    });
  }

  openRoomDialog(room?: RoomSummary) {
    forkJoin({
      wards: this.facilityService.getWards({ pageNumber: 1, pageSize: 1000 }),
      types: this.facilityService.getRoomTypes({ pageNumber: 1, pageSize: 1000 })
    }).subscribe(results => {
      const allWards = results.wards.success ? results.wards.data.items : [];
      const allTypes = results.types.success ? results.types.data.items : [];

      //  Build and open the dialog
      const data: DynamicDialogData = {
        title: room?.id ? 'Edit Room' : 'Add New Room',
        fields: [
          { key: 'roomNumber', label: 'Room Number', type: 'text', value: room?.roomNumber, required: true },
          { key: 'wardId', label: 'Ward', type: 'select', value: room?.wardId, required: true, 
            options: allWards.map(w => ({ label: w.name, value: w.id })) },
          { key: 'roomTypeId', label: 'Room Type', type: 'select', value: room?.roomTypeId, required: true, 
            options: allTypes.map(t => ({ label: t.name, value: t.id })) }
        ]
      };
      if (!room?.id) {
        data.fields.push({ key: 'numberOfBeds', label: 'Number of Beds to Generate', type: 'number', value: 1, required: true });
      }

      this.dialog.open(DynamicDialogComponent, { width: '400px', data }).afterClosed().subscribe(res => {
        if (res) {
          if (room?.id) {
            const { numberOfBeds, ...updatePayload } = res;
            this.facilityService.updateRoom(room.id, updatePayload).subscribe(() => this.loadRooms());
          } 
          else this.facilityService.createRoom(res).subscribe(() => this.loadRooms());
        }
      });
    });
  }

  openBedDialog(bed: BedSummary) {
    const data: DynamicDialogData = {
      title: `Edit Bed ${bed.bedNumber}`,
      fields: [
        { key: 'bedNumber', label: 'Bed Tag Identifier', type: 'text', value: bed.bedNumber, required: true },
        { key: 'status', label: 'Status', type: 'select', value: this.mapStatusToInteger(bed.status), required: true, 
          options: [
            { label: 'Available', value: 0 }, { label: 'Occupied', value: 1 },
            { label: 'Under Maintenance', value: 2 }, { label: 'Inactive', value: 3 }
          ]
        }
      ]
    };
    this.dialog.open(DynamicDialogComponent, { width: '400px', data }).afterClosed().subscribe(res => {
      if (res) this.facilityService.updateBed(bed.id, res).subscribe(() => this.loadBeds());
    });
  }

  // DELETES & HELPERS

  deleteWard(id: string): void {
    if (confirm('Delete this ward? All child rooms and beds will be deleted.')) {
      this.facilityService.deleteWard(id).subscribe(() => this.loadWards());
    }
  }
  deleteRoom(id: string): void {
    if (confirm('Delete this room and all its sub-generated beds?')) {
      this.facilityService.deleteRoom(id).subscribe(() => this.loadRooms());
    }
  }
  deleteRoomType(id: string): void {
    if (confirm('Delete this room type configuration?')) {
      this.facilityService.deleteRoomType(id).subscribe(() => this.loadRoomTypes());
    }
  }
  deleteBed(id: string): void {
    if (confirm('Are you sure you want to permanently delete this bed?')) {
      this.facilityService.deleteBed(id).subscribe(() => this.loadBeds());
    }
  }
  
  mapStatusToInteger(status: any): number {
    const s = status?.toString().toLowerCase().trim();
    if (s === 'occupied' || s === '1') return 1;
    if (s === 'undermaintenance' || s === 'maintenance' || s === '2') return 2;
    if (s === 'inactive' || s === '3') return 3;
    return 0;
  }
  getBedStatusName(status: string | number): string {
    if (typeof status === 'string') return status.replace(/([A-Z])/g, ' $1').trim(); 
    switch(status) {
      case 0: return 'Available'; case 1: return 'Occupied';
      case 2: return 'Maintenance'; case 3: return 'Inactive';
      default: return 'Unknown';
    }
  }
  getChipColor(status: string | number): string {
    const n = status.toString().toLowerCase();
    switch(n) {
      case 'available': case '0': return 'accent';
      case 'occupied': case '1': return 'primary';
      case 'undermaintenance': case 'maintenance': case '2': return 'warn';
      default: return ''; 
    }
  }
  viewRoomBeds(room: RoomSummary) {
    this.bedParams.search = room.roomNumber;
    this.bedParams.pageNumber = 1; 
    this.selectedTabIndex = 1; 
  }
}