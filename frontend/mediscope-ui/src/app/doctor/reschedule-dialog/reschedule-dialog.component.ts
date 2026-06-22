import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { AppointmentService } from '../../services/appointment.service';
import { DoctorAppointmentResponseDto } from '../../models/appointment.model';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reschedule-dialog',
  templateUrl: './reschedule-dialog.component.html',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, 
    MatButtonModule, MatDatepickerModule, MatNativeDateModule, MatDialogModule
  ]
})
export class RescheduleDialogComponent implements OnInit {
  rescheduleForm: FormGroup;
  minDate: Date = new Date();

  constructor(
    private fb: FormBuilder,
    private appointmentService: AppointmentService,
    public dialogRef: MatDialogRef<RescheduleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DoctorAppointmentResponseDto
  ) {
    this.rescheduleForm = this.fb.group({
      newDate: [new Date(data.startTime), Validators.required],
      newTimeString: ['', Validators.required],
      reason: ['', Validators.required]
    });
  }

  ngOnInit() {
    const current = new Date(this.data.startTime);
    const hours = current.getHours().toString().padStart(2, '0');
    const minutes = current.getMinutes().toString().padStart(2, '0');
    this.rescheduleForm.patchValue({ newTimeString: `${hours}:${minutes}` });
  }

  onSubmit() {
    if (this.rescheduleForm.invalid) return;

    const formVal = this.rescheduleForm.value;
    const date = new Date(formVal.newDate);
    const [hours, minutes] = formVal.newTimeString.split(':');
    date.setHours(parseInt(hours, 10), parseInt(minutes, 10));

    const request = {
      appointmentId: this.data.appointmentId,
      rescheduledTo: date.toISOString(),
      rescheduleReason: formVal.reason
    };

    this.appointmentService.rescheduleAppointment(this.data.appointmentId, request).subscribe(() => {
      this.dialogRef.close(true);
    });
  }
}