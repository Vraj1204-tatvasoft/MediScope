import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { AppointmentService } from '../../services/appointment.service';
import { PatientDto } from '../../models/appointment.model';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
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
  selector: 'app-book-appointment-dialog',
  templateUrl: './book-appointment-dialog.component.html',
  styleUrls: ['./book-appointment-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatDatepickerModule, MatNativeDateModule,
    MatDialogModule, MatIconModule, MatProgressSpinnerModule, FullCalendarModule
  ]
})
export class BookAppointmentDialogComponent implements OnInit {
  bookingForm: FormGroup;
  patients: PatientDto[] = [];
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
    slotMaxTime: '23:00:00',
    allDaySlot: false,
    height: '100%',
    expandRows: false,
    selectable: false,
    editable: false,
    eventInteractive: false,
    // Show busy slots only — no patient names
    events: [],
    eventContent: () => ({ html: '<div class="busy-slot"></div>' }),
    slotLabelFormat: { hour: '2-digit', minute: '2-digit', hour12: true }
  };

  constructor(
    private fb: FormBuilder,
    private appointmentService: AppointmentService,
    public dialogRef: MatDialogRef<BookAppointmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    const initDate = data?.selectedDate ? new Date(data.selectedDate) : new Date();
    const initTime = `${initDate.getHours().toString().padStart(2, '0')}:${initDate.getMinutes().toString().padStart(2, '0')}`;
    this.bookingForm = this.fb.group({
      patientId:       ['', Validators.required],
      startTime:       [data?.selectedDate || new Date(), Validators.required],
      timeString:      [initTime, Validators.required],
      durationMinutes: [30, Validators.required],
      doctorNotes:     ['']
    });
  }

  ngOnInit() {
    this.appointmentService.getDoctorPatients().subscribe({
      next: (res) => { this.patients = res.data; }
    });

    this.loadDoctorSchedule();

    this.bookingForm.get('startTime')?.valueChanges.subscribe(date => {
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
    this.appointmentService.getDoctorSchedule().subscribe({
      next: (res) => {
        const slots = res.data || [];
        this.calendarOptions = {
          ...this.calendarOptions,
          initialDate: this.data?.selectedDate || new Date(),
          events: slots
            .filter((s: any) => !['cancelled', 'rejected', 'completed'].includes(s.status?.toLowerCase()))
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
    if (this.bookingForm.invalid) return;

    const formVal = this.bookingForm.value;
    const date    = new Date(formVal.startTime);
    const [hours, minutes] = formVal.timeString.split(':');
    date.setHours(parseInt(hours, 10), parseInt(minutes, 10));

    const request = {
      patientId:       formVal.patientId,
      startTime:       date.toISOString(),
      durationMinutes: formVal.durationMinutes,
      doctorNotes:     formVal.doctorNotes
    };

    this.appointmentService.createAppointment(request).subscribe(() => {
      this.dialogRef.close(true);
    });
  }
}