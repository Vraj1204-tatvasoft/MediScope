import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PatientAppointmentResponseDto } from '../../models/appointment.model';
import { AppointmentService } from '../../services/appointment.service';

import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { RescheduleDialogComponent } from '../../doctor/reschedule-dialog/reschedule-dialog.component';

@Component({
  selector: 'app-patient-appointment',
  templateUrl: './patient-appointment.component.html',
  styleUrls: ['./patient-appointment.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatDialogModule
  ]
})
export class PatientAppointmentComponent implements OnInit {
  allAppointments: PatientAppointmentResponseDto[] = [];
  upcomingAppointments: PatientAppointmentResponseDto[] = [];
  historicalAppointments: PatientAppointmentResponseDto[] = [];
  
  currentUserId: string = '';
  displayedColumns: string[] = ['date', 'doctor', 'duration', 'status', 'notes'];

  constructor(
    private appointmentService: AppointmentService,
    private authService: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.currentUserId = this.authService.currentUser()?.id ?? '';
    this.loadAppointments();
  }

  loadAppointments() {
    this.appointmentService.getPatientAppointments().subscribe({
      next: (res) => {
        this.allAppointments = res.data || [];
        this.categorizeAppointments();
      },
      error: (err) => console.error('Failed to load appointments', err)
    });
  }

  categorizeAppointments() {
    const now = new Date();
    
    // Upcoming: Future dates AND not cancelled/rejected
    this.upcomingAppointments = this.allAppointments.filter(a => 
      new Date(a.endTime) >= now && 
      !['cancelled', 'rejected'].includes(a.status.toLowerCase())
    );

    // History: Past dates OR cancelled/rejected status
    this.historicalAppointments = this.allAppointments.filter(a => 
      new Date(a.endTime) < now || 
      ['cancelled', 'rejected'].includes(a.status.toLowerCase())
    );
  }

  openRescheduleModal(appointment: PatientAppointmentResponseDto) {
    const dialogRef = this.dialog.open(RescheduleDialogComponent, {
      width: '450px',
      // Map the property so the dialog knows the display name
      data: { ...appointment, patientName: appointment.doctorName } 
    });
    dialogRef.afterClosed().subscribe(res => { if(res) this.loadAppointments(); });
  }

  acceptAppointment(appointment: PatientAppointmentResponseDto) {
    this.appointmentService.respondToAppointment(appointment.appointmentId, {
      appointmentId: appointment.appointmentId, action: 'accepted'
    }).subscribe({
      next: () => this.loadAppointments(),
      error: (err) => alert(err.error?.message)
    });
  }

  rejectAppointment(appointment: PatientAppointmentResponseDto) {
    this.appointmentService.respondToAppointment(appointment.appointmentId, {
      appointmentId: appointment.appointmentId, action: 'rejected'
    }).subscribe({
      next: () => this.loadAppointments(),
      error: (err) => alert(err.error?.message)
    });
  }

  cancelAppointment(appointment: PatientAppointmentResponseDto) {
    if(confirm('Are you sure you want to cancel your appointment with Dr. ' + appointment.doctorName + '?')) {
      this.appointmentService.cancelAppointment(appointment.appointmentId, 'Cancelled by patient').subscribe({
        next: () => this.loadAppointments(),
        error: (err) => alert(err.error?.message)
      });
    }
  }
}