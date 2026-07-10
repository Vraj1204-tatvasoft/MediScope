import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

// Material Imports
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';

// Services, Models & Components
import { PatientAppointmentResponseDto } from '../../models/appointment.model';
import { AppointmentService } from '../../services/appointment.service';
import { AuthService } from '../../core/services/auth.service';
import { RescheduleDialogComponent } from '../../doctor/reschedule-dialog/reschedule-dialog.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component'; // ── 🛠️ IMPORTED DIALOG

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
      }
    });
  }

  categorizeAppointments() {
    const now = new Date();
    this.upcomingAppointments = this.allAppointments.filter(a => 
      new Date(a.endTime) >= now && 
      !['cancelled', 'rejected'].includes(a.status.toLowerCase())
    );
    this.historicalAppointments = this.allAppointments.filter(a => 
      new Date(a.endTime) < now || 
      ['cancelled', 'rejected'].includes(a.status.toLowerCase())
    );
  }

  openRescheduleModal(appointment: PatientAppointmentResponseDto) {
    const dialogRef = this.dialog.open(RescheduleDialogComponent, {
      width: '1000px',
      data: { ...appointment, patientName: appointment.doctorName } 
    });
    dialogRef.afterClosed().subscribe(res => { 
      if(res) this.loadAppointments(); 
    });
  }

  acceptAppointment(appointment: PatientAppointmentResponseDto) {
    this.appointmentService.respondToAppointment(appointment.appointmentId, {
      appointmentId: appointment.appointmentId, action: 'accepted'
    }).subscribe(() => {
      this.loadAppointments();
    });
  }

  rejectAppointment(appointment: PatientAppointmentResponseDto) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Decline Appointment',
        message: `Are you sure you want to decline the proposed time with Dr. ${appointment.doctorName}?`,
        confirmText: 'Yes, Decline',
        cancelText: 'Go Back'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.appointmentService.respondToAppointment(appointment.appointmentId, {
          appointmentId: appointment.appointmentId, action: 'rejected'
        }).subscribe(() => this.loadAppointments());
      }
    });
  }

  cancelAppointment(appointment: PatientAppointmentResponseDto) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Cancel Appointment',
        message: `Are you sure you want to cancel your upcoming appointment with Dr. ${appointment.doctorName}? This action cannot be undone.`,
        confirmText: 'Yes, Cancel it',
        cancelText: 'Keep Appointment'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.appointmentService.cancelAppointment(appointment.appointmentId, 'Cancelled by patient')
          .subscribe(() => this.loadAppointments());
      }
    });
  }
}