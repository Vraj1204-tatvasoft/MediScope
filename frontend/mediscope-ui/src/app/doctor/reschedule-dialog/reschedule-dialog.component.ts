import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { AppointmentService } from '../../services/appointment.service';
import { AuthService } from '../../core/services/auth.service'; 

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';

import { FullCalendarComponent, FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';

@Component({
  selector: 'app-reschedule-dialog',
  templateUrl: './reschedule-dialog.component.html',
  styleUrls: ['./reschedule-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatDatepickerModule, MatNativeDateModule,
    MatDialogModule, MatIconModule, MatProgressSpinnerModule, FullCalendarModule
  ]
})
export class RescheduleDialogComponent implements OnInit {
  rescheduleForm: FormGroup;
  minDate: Date = new Date();
  isLoadingSchedule = true;
  @ViewChild('calendar') calendarComponent!: FullCalendarComponent;
  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'timeGridDay',
    headerToolbar: {
      left: 'prev,next',
      center: 'title',
      right: 'timeGridDay'
    },
    slotMinTime: '00:00:00',
    slotMaxTime: '24:00:00',
    allDaySlot: false,
    height: '100%',
    expandRows: false,
    selectable: false,
    editable: false,
    eventInteractive: false,
    events: [],
    eventContent: () => ({ html: '<div class="busy-slot">Busy</div>' }),
    slotLabelFormat: { hour: '2-digit', minute: '2-digit', hour12: true }
  };

  constructor(
    private fb: FormBuilder,
    private appointmentService: AppointmentService,
    private authService: AuthService, 
    public dialogRef: MatDialogRef<RescheduleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any 
  ) {
    this.rescheduleForm = this.fb.group({
      newDate:       [new Date(data.startTime), Validators.required],
      newTimeString: ['', Validators.required],
      reason:        ['', Validators.required]
    });
  }

  ngOnInit() {
    const current = new Date(this.data.startTime);
    const hours   = current.getHours().toString().padStart(2, '0');
    const minutes = current.getMinutes().toString().padStart(2, '0');
    this.rescheduleForm.patchValue({ newTimeString: `${hours}:${minutes}` });
    
    this.loadDoctorSchedule();

    this.rescheduleForm.get('newDate')?.valueChanges.subscribe(date => {
      if (date) {
        const newDate = new Date(date);
        this.calendarOptions = {
          ...this.calendarOptions,
          initialDate: new Date(date)
        };
        if (this.calendarComponent) {
          this.calendarComponent.getApi().gotoDate(newDate);
        }
      }
    });
  }

  loadDoctorSchedule() {
    this.isLoadingSchedule = true;
    
    const targetDoctorId = this.data.doctorId || this.authService.currentUser()?.id;

    if (!targetDoctorId) {
      console.error("Could not determine Doctor ID for schedule.");
      this.isLoadingSchedule = false;
      return;
    }

    this.appointmentService.getDoctorAvailability(targetDoctorId).subscribe({
      next: (res) => {
        const slots = res.data || [];

        this.calendarOptions = {
          ...this.calendarOptions,
          initialDate: new Date(this.data.startTime),
          events: slots
            .filter((s: any) =>
              !['cancelled', 'rejected', 'completed'].includes(s.status?.toLowerCase())
            )
            .map((slot: any) => ({
              start:           slot.startTime,
              end:             slot.endTime,
              backgroundColor: '#fde8e8',
              borderColor:     '#f87171',
              textColor:       '#b91c1c',
              display:         'block'
            }))
        };
        this.isLoadingSchedule = false;
        setTimeout(() => {
          if (this.calendarComponent) {
            this.calendarComponent.getApi().updateSize();
          }
        }, 100); 
      },
      error: () => { this.isLoadingSchedule = false; }
    });
  }

  onSubmit() {
    if (this.rescheduleForm.invalid) return;

    const formVal = this.rescheduleForm.value;
    const date    = new Date(formVal.newDate);
    const [hours, minutes] = formVal.newTimeString.split(':');
    date.setHours(parseInt(hours, 10), parseInt(minutes, 10));

    const request = {
      appointmentId:    this.data.appointmentId,
      rescheduledTo:    date.toISOString(),
      rescheduleReason: formVal.reason
    };

    this.appointmentService.rescheduleAppointment(this.data.appointmentId, request)
      .subscribe(() => this.dialogRef.close(true));
  }
}