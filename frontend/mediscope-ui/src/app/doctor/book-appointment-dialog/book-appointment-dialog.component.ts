import { Component, Inject, OnInit } from '@angular/core';
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
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-book-appointment-dialog',
  templateUrl: './book-appointment-dialog.component.html',
  styleUrls: ['./book-appointment-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatDatepickerModule, MatNativeDateModule, MatDialogModule
  ]
})
export class BookAppointmentDialogComponent implements OnInit {
  bookingForm: FormGroup;
  patients: PatientDto[] = []; 
  minDate: Date = new Date(); // Blocks past dates

  constructor(
    private fb: FormBuilder,
    private appointmentService: AppointmentService,
    public dialogRef: MatDialogRef<BookAppointmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    const initDate = data?.selectedDate ? new Date(data.selectedDate) : new Date();
    const initTime = `${initDate.getHours().toString().padStart(2, '0')}:${initDate.getMinutes().toString().padStart(2, '0')}`;
    this.bookingForm = this.fb.group({
      patientId: ['', Validators.required],
      startTime: [data?.selectedDate || new Date(), Validators.required],
      timeString: [initTime, Validators.required],
      durationMinutes: [30, Validators.required],
      doctorNotes: ['']
    });
  }

  ngOnInit() {
    // Fetch dynamic patients
    this.appointmentService.getDoctorPatients().subscribe({
      next: (res) => { this.patients = res.data; }
    });
  }

  onSubmit() {
    if (this.bookingForm.invalid) return;

    const formVal = this.bookingForm.value;
    const date = new Date(formVal.startTime);
    const [hours, minutes] = formVal.timeString.split(':');
    date.setHours(parseInt(hours, 10), parseInt(minutes, 10));

    const request = {
      patientId: formVal.patientId,
      startTime: date.toISOString(),
      durationMinutes: formVal.durationMinutes,
      doctorNotes: formVal.doctorNotes
    };

    this.appointmentService.createAppointment(request).subscribe(() => {
      this.dialogRef.close(true);
    });
  }
}