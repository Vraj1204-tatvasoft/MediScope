import { Component, inject, OnInit, signal, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatError, MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { debounceTime, distinctUntilChanged, forkJoin, Subject } from 'rxjs';
import { ManagePatientsService } from '../../services/manage-patients.service';
import { DoctorService } from '../../services/doctor.service';
import { AdmissionService } from '../../services/admission.service';
import { ManageRoomService } from '../../services/manage-room.service';
import { AdmissionStatus, AdmissionSummary, AvailableBedResponse, DischargePatientPayload, RoomPatient } from '../../models/admission.model';
import { BedSummary, PaginationParams, RoomSummary, WardSummary } from '../../models/manage-room.model';
import { ActivatedRoute, Router } from '@angular/router';
import { RoomCalendarComponent } from '../room-calendar/room-calendar.component';

@Component({
  selector: 'app-admissions',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, MatCardModule, MatTableModule, MatButtonModule, 
    MatIconModule, MatChipsModule, MatMenuModule, MatPaginatorModule, 
    MatProgressBarModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDialogModule,
    MatDatepickerModule, MatNativeDateModule, RoomCalendarComponent, MatError, MatNativeDateModule
  ],
  templateUrl: './admissions.component.html',
  styleUrls: ['./admissions.component.css']
})
export class AdmissionsComponent implements OnInit {
  private readonly admissionService = inject(AdmissionService);
  private readonly roomService = inject(ManageRoomService);
  private readonly patientService = inject(ManagePatientsService);
  private readonly doctorService = inject(DoctorService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  
  admissions = signal<AdmissionSummary[]>([]);
  isLoading = signal<boolean>(false);
  totalActive = signal<number>(0);
  totalDischarged = signal<number>(0);
  totalCount = signal<number>(0);
  searchSubject = new Subject<string>();
  columns: string[] = ['admissionNumber', 'patientName', 'doctorName', 'location', 'admissionDate', 'status', 'actions'];
  params: PaginationParams & { status?: number, search?: string } = { pageNumber: 1, pageSize: 10 };

  @ViewChild('admitDialogTpl') admitDialogTpl!: TemplateRef<any>;
  @ViewChild('transferDialogTpl') transferDialogTpl!: TemplateRef<any>;
  @ViewChild('dischargeDialogTpl') dischargeDialogTpl!: TemplateRef<any>;
  
  @ViewChild('bedAllocationDialogTpl') bedAllocationDialogTpl!: TemplateRef<any>;
  
  activeDialogRef: MatDialogRef<any> | null = null;
  allocationDialogRef: MatDialogRef<any> | null = null; 
  selectedAdmission: AdmissionSummary | null = null;

  admitForm!: FormGroup;
  transferForm!: FormGroup;
  dischargeForm!: FormGroup;

  wards = signal<WardSummary[]>([]);
  roomTypes = signal<any[]>([]);
  allRooms = signal<RoomSummary[]>([]);
  filteredRooms = signal<RoomSummary[]>([]);
  allBeds = signal<BedSummary[]>([]);
  filteredBeds = signal<BedSummary[]>([]);
  patients = signal<any[]>([]); 
  doctors = signal<any[]>([]);
  today = new Date();
  isEditMode = false;
  editingAdmissionId: string | null = null;
  allocationFilters = { wardId: null as string | null, roomType: null as string | null };
  filteredRoomsDataSource = signal<any[]>([]);
  selectedAllocationRoom: any | null = null;
  roomPatients: RoomPatient[] = [];
  selectedAllocationDetails: {
    wardName: string;
    roomNumber: string;
    bedNumber: string;
  } | null = null;
  get minExpectedDischargeDate(): Date {
    const admitDate = this.admitForm?.get('admissionDate')?.value;
    return admitDate ? new Date(admitDate) : this.today;
  }
  minDischargeDate!: Date;
  maxDischargeDate: Date = new Date();
  ngOnInit(): void {
    this.initForms();
    this.loadDashboardData();
    
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe((searchValue) => {
      this.params.search = searchValue;
      this.onFilterChange();
    });

    this.route.queryParams.subscribe(params => {
      const patientId = params['prefillPatient'];
      if (patientId) {
        setTimeout(() => this.openAdmitDialog(patientId), 100);
        this.router.navigate([], { queryParams: { prefillPatient: null }, queryParamsHandling: 'merge' });
      }
    });
    this.route.queryParams.subscribe(params => {
      if (params['action'] === 'admit') {
        setTimeout(() => {
          this.openAdmitDialog(); 
          this.router.navigate([], {
            queryParams: { action: null },
            queryParamsHandling: 'merge',
            replaceUrl: true 
          });
        }, 100); 
      }
    });
  }

  private initForms() {
    this.admitForm = this.fb.group({
      patientId: ['', Validators.required],
      doctorId: ['', Validators.required],
      wardId: ['', Validators.required], // Still required, but populated by the Allocation Grid
      roomId: ['', Validators.required], // Still required, but populated by the Allocation Grid
      bedId: ['', Validators.required],  // Still required, but populated by the Allocation Grid
      admissionReason: ['', Validators.required],
      admissionDate: [null, [Validators.required, this.futureDateValidator()]],
      admissionTime: ['09:00', Validators.required],
      expectedDischargeDate: [null, Validators.required],
      expectedDischargeTime: [null, Validators.required],
      remarks: ['']
    },
      {
        validators: this.expectedDischargeValidator()
      });

    this.transferForm = this.fb.group({
      newWardId: ['', Validators.required],
      newRoomId: ['', Validators.required],
      newBedId: ['', Validators.required],
      transferReason: ['', Validators.required]
    });

    this.dischargeForm = this.fb.group({
      dischargeDate: [null, Validators.required],
      dischargeTime: ['', Validators.required],
      dischargeNotes: ['']
    });
  }
  private getCleanParams(): any {
    const cleanParams: any = { 
      pageNumber: this.params.pageNumber, 
      pageSize: this.params.pageSize 
    };
    if (this.params.search && this.params.search.trim() !== '') {
      cleanParams.search = this.params.search.trim();
    }
    if (this.params.status !== undefined && this.params.status !== null) {
      cleanParams.status = this.params.status;
    }

    return cleanParams;
  }
  loadDashboardData(): void {
    this.isLoading.set(true);

    forkJoin({
      grid: this.admissionService.getAdmissions(this.getCleanParams()),
      
      // activeStats: this.admissionService.getAdmissions({ pageNumber: 1, pageSize: 1, status: 0 } as any),
      // dischargedStats: this.admissionService.getAdmissions({ pageNumber: 1, pageSize: 1, status: 1 } as any)
    }).subscribe(res => {
      if(res.grid.success) {
        this.admissions.set(res.grid.data.items);
        this.totalCount.set(res.grid.data.totalCount);
      }
      // if(res.activeStats.success) this.totalActive.set(res.activeStats.data.totalCount);
      // if(res.dischargedStats.success) this.totalDischarged.set(res.dischargedStats.data.totalCount);
      
      this.isLoading.set(false);
    });
  }

  loadGridOnly() {
    this.isLoading.set(true);
    
    this.admissionService.getAdmissions(this.getCleanParams()).subscribe(res => {
      if(res.success) {
        this.admissions.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
      }
      this.isLoading.set(false);
    });
  }

  onPage(e: PageEvent) {
    this.params.pageNumber = e.pageIndex + 1;
    this.params.pageSize = e.pageSize;
    this.loadGridOnly();
  }

  onFilterChange() {
    this.params.pageNumber = 1;
    this.loadGridOnly();
  }

  // DIALOG ACTIONS & CASCADING LOGIC
  
  private loadFacilityData(targetDate?: Date, targetDischargeDate?: Date) {
    // 1. Pass the target date into the parameters
    const dropdownParams: any = { 
      pageNumber: 1, 
      pageSize: 1000,
      admissionDate: targetDate ? targetDate.toISOString() : null,
      expectedDischargeDate: targetDischargeDate ? targetDischargeDate.toISOString() : null
    };

    forkJoin({
      w: this.roomService.getWards(dropdownParams),
      r: this.roomService.getRooms(dropdownParams), 
      b: this.roomService.getBeds(dropdownParams),
      rt: this.roomService.getRoomTypes(dropdownParams), 
      docs: this.doctorService.getAllDoctors(),
      pats: this.patientService.getAdminPatients(1, 1000) 
    }).subscribe(res => {
      if(res.w.success) this.wards.set(res.w.data.items);
      if(res.r.success) {
        this.allRooms.set(res.r.data.items);
        this.filteredRoomsDataSource.set(res.r.data.items); 
      }
      if(res.b.success) this.allBeds.set(res.b.data.items);
      if(res.rt?.success) this.roomTypes.set(res.rt.data.items); 
      if(res.docs) this.doctors.set(res.docs); 
      if(res.pats.success) this.patients.set(res.pats.data.patients.items); 
    });
  }
  
  onWardChange(wardId: string, form: FormGroup) {
    this.filteredRooms.set(this.allRooms().filter(r => r.ward_Id === wardId));
    this.filteredBeds.set([]);
    if (form === this.transferForm) form.patchValue({ newRoomId: null, newBedId: null });
  }

  onRoomChange(roomId: string, form: FormGroup) {
    const selectedRoom = this.allRooms().find(r => r.id === roomId);
    this.filteredBeds.set(this.allBeds().filter(b => b.roomNumber === selectedRoom?.roomNumber && b.status === 'Available'));
    if (form === this.transferForm) form.patchValue({ newBedId: null });
  }
  onSearchChange(event: Event) {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }
  openAdmitDialog(prefillPatientId?: string) {
    this.loadFacilityData();
    this.isEditMode = false;
    this.editingAdmissionId = null;
    this.admitForm.reset({ admissionDate: new Date(),
        admissionTime: '09:00',
        expectedDischargeTime: '09:00'
     });
    this.selectedAllocationDetails = null;
    const patientCtrl = this.admitForm.get('patientId');
    
    if (prefillPatientId) {
      patientCtrl?.setValue(prefillPatientId);
      patientCtrl?.disable();
    } else {
      patientCtrl?.enable();
    }

    this.activeDialogRef = this.dialog.open(this.admitDialogTpl, { width: '600px' });
  }
  openEditAdmission(admission: AdmissionSummary) {
    this.isEditMode = true;
    this.editingAdmissionId = admission.id;
  
    this.loadFacilityData();
  
    this.admissionService
        .getAdmissionById(admission.id)
        .subscribe(res => {
          const data = res.data;
          const admissionDate = new Date(data.admissionDate);
          const dischargeDate =
              data.expectedDischargeDate
                  ? new Date(data.expectedDischargeDate)
                  : null;
          this.admitForm.reset({
            patientId: data.patientId,
            doctorId: data.doctorId,
            wardId: data.wardId,
            roomId: data.roomId,
            bedId: data.bedId,
            admissionReason: data.admissionReason,
            admissionDate: admissionDate,
            admissionTime: admissionDate.toTimeString().substring(0, 5),
            expectedDischargeDate: dischargeDate,
            expectedDischargeTime: dischargeDate ? dischargeDate.toTimeString().substring(0, 5) : '09:00',
            remarks: data.remarks
          });
          this.admitForm.get('patientId')?.disable();
          this.selectedAllocationDetails = {
            wardName: data.wardName,
            roomNumber: data.roomNumber,
            bedNumber: data.bedNumber
          };
          this.activeDialogRef = this.dialog.open(
            this.admitDialogTpl,
            {
              width: '600px'
            });
        });
  }
  // BED ALLOCATION LOGIC 

  openBedAllocationModal() {
    const rawAdmitDate = this.admitForm.get('admissionDate')?.value;
    const rawAdmitTime = this.admitForm.get('admissionTime')?.value; 
    let targetDate: Date | undefined = undefined;

    if (rawAdmitDate) {
      targetDate = new Date(rawAdmitDate);
      if (rawAdmitTime) {
        const [hours, minutes] = rawAdmitTime.split(':');
        targetDate.setHours(Number(hours), Number(minutes), 0, 0);
      }
    }

    const rawDischargeDate = this.admitForm.get('expectedDischargeDate')?.value;
    const rawDischargeTime = this.admitForm.get('expectedDischargeTime')?.value;
    let targetDischargeDate: Date | undefined = undefined;
    if (!rawAdmitDate || !rawAdmitTime || !rawDischargeDate || !rawDischargeTime) {
      return; 
    }
    if (rawDischargeDate) {
      targetDischargeDate = new Date(rawDischargeDate);
      if (rawDischargeTime) {
        const [hours, minutes] = rawDischargeTime.split(':');
        targetDischargeDate.setHours(Number(hours), Number(minutes), 0, 0);
      }
    }

    this.loadFacilityData(targetDate, targetDischargeDate);

    this.allocationFilters = { wardId: null, roomType: null };
    this.selectedAllocationRoom = null;
    this.applyAllocationFilters(); 

    this.allocationDialogRef = this.dialog.open(this.bedAllocationDialogTpl, { 
      width: '800px',
      disableClose: true 
    });
  }

  private expectedDischargeValidator() {
    return (group: AbstractControl): ValidationErrors | null => {
  
      const admissionDate = group.get('admissionDate')?.value;
      const admissionTime = group.get('admissionTime')?.value;
      const dischargeDate = group.get('expectedDischargeDate')?.value;
      const dischargeTime = group.get('expectedDischargeTime')?.value;
  
      // Expected discharge is optional
      if (!dischargeDate) {
        return null;
      }
  
      if (!admissionDate || !admissionTime || !dischargeTime) {
        return null;
      }
  
      const admission = new Date(admissionDate);
      const [aHour, aMinute] = admissionTime.split(':').map(Number);
      admission.setHours(aHour, aMinute, 0, 0);
  
      const discharge = new Date(dischargeDate);
      const [dHour, dMinute] = dischargeTime.split(':').map(Number);
      discharge.setHours(dHour, dMinute, 0, 0);
  
      const differenceInMinutes =
        (discharge.getTime() - admission.getTime()) / (1000 * 60);
  
      return differenceInMinutes >= 30
        ? null
        : { minimumDischargeGap: true };
    };
  }
  
  applyAllocationFilters() {
    let filtered = this.allRooms();

    if (this.allocationFilters.wardId) {
      filtered = filtered.filter(r => r.ward_Id === this.allocationFilters.wardId);
    }
    
    if (this.allocationFilters.roomType) {
      filtered = filtered.filter((r: any) => r.room_Type_Id === this.allocationFilters.roomType); 
    }

    this.filteredRoomsDataSource.set(filtered);
  }

  toggleRoomSelection(room: any) {

    if (this.selectedAllocationRoom?.id === room.id) {
  
      this.selectedAllocationRoom = null;
      this.roomPatients = [];
  
      return;
    }
  
    this.selectedAllocationRoom = room;
  
    this.admissionService.getActivePatients(room.id).subscribe({
      next: (res) => {
        this.roomPatients = res.data;
      },
      error: () => {
        this.roomPatients = [];
      }
    });
  }
  saveBedAllocation() {
    if (!this.selectedAllocationRoom) {
      return;
    }
  
    // 1. Reconstruct the requested start and end dates from the form
    const formVals = this.admitForm.value;
    
    const requestedStart = new Date(formVals.admissionDate);
    if (formVals.admissionTime) {
      const [startHrs, startMins] = formVals.admissionTime.split(':');
      requestedStart.setHours(Number(startHrs), Number(startMins), 0, 0);
    }
  
    const requestedEnd = new Date(formVals.expectedDischargeDate);
    if (formVals.expectedDischargeTime) {
      const [endHrs, endMins] = formVals.expectedDischargeTime.split(':');
      requestedEnd.setHours(Number(endHrs), Number(endMins), 0, 0);
    }
  
    // 2. Ask the backend for a specific free bed
    this.admissionService.getFirstAvailableBed(
      this.selectedAllocationRoom.id, 
      requestedStart.toISOString(), 
      requestedEnd.toISOString()
    ).subscribe({
      // 1. Accept 'any' to satisfy the rigid baseHttp signature
      next: (response: any) => { 
        
        // 2. Cast the response to your interface so autocomplete and compilation work!
        const safeBed = response as AvailableBedResponse; 
    
        if (!safeBed || !safeBed.id) {
           alert('No bed available in this room.');
           return;
        }
    
        // 3. TypeScript now knows these properties exist and will compile perfectly
        this.admitForm.patchValue({
          wardId: this.selectedAllocationRoom.ward_Id, 
          roomId: this.selectedAllocationRoom.id,
          bedId: safeBed.id 
        });
    
        this.selectedAllocationDetails = {
          wardName: this.wards().find(w => w.id === this.selectedAllocationRoom.ward_Id)?.name ?? '',
          roomNumber: this.selectedAllocationRoom.roomNumber,
          bedNumber: safeBed.bedNumber 
        };
    
        this.allocationDialogRef?.close();
      },
      error: (err) => {
        console.error("API ERROR:", err);
        alert('Failed to allocate bed. Please select another room.');
      }
    });
  }
  submitAdmission() {
    if (!this.admitForm.valid) {
      return;
    }
    const rawValue = this.admitForm.getRawValue();
  
    const admissionDate = new Date(rawValue.admissionDate);
    const [admissionHour, admissionMinute] = rawValue.admissionTime.split(':').map(Number);
  
    admissionDate.setHours(admissionHour);  
    admissionDate.setMinutes(admissionMinute);
    admissionDate.setSeconds(0);
    let expectedDischarge: string | null = null;
  
    if (rawValue.expectedDischargeDate) {
      const dischargeDate = new Date(rawValue.expectedDischargeDate);
      const [dischargeHour, dischargeMinute] = rawValue.expectedDischargeTime.split(':').map(Number);
  
      dischargeDate.setHours(dischargeHour);
      dischargeDate.setMinutes(dischargeMinute);
      dischargeDate.setSeconds(0);
  
      expectedDischarge = dischargeDate.toISOString();
    }
  
    const payload = {
      patientId: rawValue.patientId,
      doctorId: rawValue.doctorId,
      wardId: rawValue.wardId,
      roomId: rawValue.roomId,
      bedId: rawValue.bedId,
      admissionReason: rawValue.admissionReason,
      admissionDate: admissionDate.toISOString(),
      expectedDischargeDate: expectedDischarge,
      remarks: rawValue.remarks || null
    };
  
    if (this.isEditMode) {
      this.admissionService
          .updateAdmission(this.editingAdmissionId!, payload)
          .subscribe(() => {
              this.activeDialogRef?.close();
              this.loadDashboardData();
          });
    } else {
      this.admissionService
          .admitPatient(payload)
          .subscribe(() => {
              this.activeDialogRef?.close();
              this.loadDashboardData();
          });
    }
  }

  openTransferDialog(admission: AdmissionSummary) {
    this.selectedAdmission = admission;
    this.loadFacilityData();
    this.transferForm.reset();
    this.filteredRooms.set([]);
    this.filteredBeds.set([]);
    this.activeDialogRef = this.dialog.open(this.transferDialogTpl, { width: '500px' });
  }

  submitTransfer() {
    if (this.transferForm.valid && this.selectedAdmission) {
      this.admissionService.transferPatient(this.selectedAdmission.id, this.transferForm.value).subscribe(() => {
        this.activeDialogRef?.close();
        this.loadGridOnly();
      });
    }
  }

  openDischargeDialog(admission: any) {
    this.selectedAdmission = admission;
    
    this.minDischargeDate = new Date(admission.admissionDate); 
    this.maxDischargeDate = new Date();                        
    const now = new Date();
    
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    const currentTime = `${hours}:${minutes}`;
  
    this.dischargeForm.reset();
    this.dischargeForm.patchValue({
      dischargeDate: now,
      dischargeTime: currentTime,
      dischargeNotes: ''
    });
  
    this.activeDialogRef = this.dialog.open(this.dischargeDialogTpl, { width: '500px' });
  }
  
  submitDischarge() {
    this.dischargeForm.markAllAsTouched(); 
  
    if (this.dischargeForm.valid && this.selectedAdmission) {
      const formVals = this.dischargeForm.value;
      
      const combinedDate = new Date(formVals.dischargeDate);
      const [hours, minutes] = formVals.dischargeTime.split(':');
      combinedDate.setHours(Number(hours), Number(minutes), 0, 0);
  
      const payload = {
        dischargeNotes: formVals.dischargeNotes || '', 
        dischargeDate: combinedDate.toISOString()      
      };
  
      this.admissionService.dischargePatient(this.selectedAdmission.id, payload).subscribe({
        next: () => {
          this.activeDialogRef?.close();
          this.loadDashboardData(); 
        },
        error: (err) => {
          console.error("Discharge failed:", err);
          alert("Failed to discharge patient.");
        }
      });
    }
  }

  getStatusName(status: AdmissionStatus): string {
    return status === AdmissionStatus.Active ? 'Active' : 
           status === AdmissionStatus.Scheduled ? 'Scheduled' :
           status === AdmissionStatus.Discharged ? 'Discharged' : 'Cancelled';
  }

  getStatusColor(status: AdmissionStatus): string {
    return status === AdmissionStatus.Active ? 'primary' : 
           status === AdmissionStatus.Discharged ? 'accent' : 
           status === AdmissionStatus.Scheduled ? 'warn' : 
           'warn' ;
  }

  futureDateValidator() {
    return (control: any) => {
      const selected = new Date(control.value);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return selected >= today ? null : { pastDate: true };
    };
  }
  checkInPatient(admission: any) {
    if (admission && admission.id) {
      this.admissionService.checkInPatient(admission.id).subscribe(() => {
        this.loadGridOnly();
      });
    }
  }
  cancelAdmission(admission: any) {
    if (confirm(`Are you sure you want to cancel the scheduled admission for ${admission.patientName}?`)) {
      this.admissionService.cancelAdmission(admission.id).subscribe(() => {
        this.loadGridOnly(); 
      });
    }
  }
} 