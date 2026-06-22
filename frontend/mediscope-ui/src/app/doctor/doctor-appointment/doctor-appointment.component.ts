import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatMenuModule, MatMenuTrigger } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';

import { AppointmentService } from '../../services/appointment.service';
import { AuthService } from '../../core/services/auth.service';
import { DoctorAppointmentResponseDto } from '../../models/appointment.model';
import { BookAppointmentDialogComponent } from '../book-appointment-dialog/book-appointment-dialog.component';
import { RescheduleDialogComponent } from '../reschedule-dialog/reschedule-dialog.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component'; 

@Component({
  selector: 'app-doctor-appointment',
  templateUrl: './doctor-appointment.component.html',
  styleUrls: ['./doctor-appointment.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatDialogModule,
    MatMenuModule,
    MatDividerModule,
    FullCalendarModule
  ]
})
export class DoctorAppointmentComponent implements OnInit {
  
  allSlots: DoctorAppointmentResponseDto[] = [];
  historicalAppointments: DoctorAppointmentResponseDto[] = [];
  currentUserId: string = '';
  stats = { total: 0, accepted: 0, pending: 0, available: 0 };
  displayedColumns: string[] = ['date', 'time', 'patient', 'duration', 'status'];

  @ViewChild(MatMenuTrigger) menuTrigger!: MatMenuTrigger;
  menuPosition = { x: '0', y: '0' };
  selectedAppointment!: DoctorAppointmentResponseDto;

  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'timeGridWeek',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay'
    },
    slotMinTime: '00:00:00',
    slotMaxTime: '23:59:00',
    allDaySlot: false,
    selectable: true,
    
    dateClick: this.handleDateClick.bind(this),
    eventClick: this.handleEventClick.bind(this),
    events: []
  };

  constructor(
    private appointmentService: AppointmentService,
    private dialog: MatDialog,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.currentUserId = this.authService.currentUser()?.id ?? '';
    this.loadSchedule();
  }

  loadSchedule() {
    this.appointmentService.getDoctorSchedule().subscribe({
      next: (res) => {
        this.allSlots = res.data || [];
        this.calculateStats();
        this.extractHistory();
        
        // Map DTOs to FullCalendar Event format
        this.calendarOptions.events = this.allSlots
          .filter(s => s.status !== 'cancelled' && s.status !== 'rejected')
          .map(slot => ({
            id: slot.appointmentId,
            title: slot.patientName || 'Reserved',
            start: slot.startTime,
            end: slot.endTime,
            backgroundColor: this.getStatusBgColor(slot.status),
            borderColor: this.getStatusBorderColor(slot.status),
            textColor: this.getStatusTextColor(slot.status),
            extendedProps: { originalData: slot }
          }));
      }
    });
  }

  calculateStats() {
    const upcoming = this.allSlots.filter(s => new Date(s.endTime) >= new Date() && s.status !== 'cancelled');
    this.stats = {
      total: upcoming.length,
      accepted: upcoming.filter(s => s.status.toLowerCase() === 'accepted').length,
      pending: upcoming.filter(s => s.status.toLowerCase() === 'pending' || s.status.toLowerCase() === 'rescheduled').length,
      available: 0 
    };
  }

  extractHistory() {
    this.historicalAppointments = this.allSlots.filter(s => new Date(s.endTime) < new Date() || s.status === 'cancelled');
  }

  handleDateClick(arg: any) {
    const dialogRef = this.dialog.open(BookAppointmentDialogComponent, {
      width: '500px',
      data: { selectedDate: arg.date },
      panelClass: 'mediscope-dialog'
    });
    dialogRef.afterClosed().subscribe(res => { if (res) this.loadSchedule(); });
  }

  handleEventClick(clickInfo: any) {
    this.selectedAppointment = clickInfo.event.extendedProps.originalData;
    this.menuPosition.x = clickInfo.jsEvent.clientX + 'px';
    this.menuPosition.y = clickInfo.jsEvent.clientY + 'px';
    this.menuTrigger.openMenu();
  }

  openBookingModal() {
    const dialogRef = this.dialog.open(BookAppointmentDialogComponent, {
      width: '500px',
      data: { selectedDate: new Date() },
      panelClass: 'mediscope-dialog'
    });
    dialogRef.afterClosed().subscribe(res => { if (res) this.loadSchedule(); });
  }

  openRescheduleModal(appointment: DoctorAppointmentResponseDto) {
    const dialogRef = this.dialog.open(RescheduleDialogComponent, {
      width: '450px',
      data: appointment
    });
    dialogRef.afterClosed().subscribe(res => { if(res) this.loadSchedule(); });
  }

  acceptAppointment(appointment: DoctorAppointmentResponseDto) {
    this.appointmentService.respondToAppointment(appointment.appointmentId, {
      appointmentId: appointment.appointmentId, action: 'accepted'
    }).subscribe(() => this.loadSchedule());
  }

  rejectAppointment(appointment: DoctorAppointmentResponseDto) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Decline Request',
        message: `Are you sure you want to decline the proposed time for ${appointment.patientName}?`,
        confirmText: 'Yes, Decline',
        cancelText: 'Go Back'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.appointmentService.respondToAppointment(appointment.appointmentId, {
          appointmentId: appointment.appointmentId, action: 'rejected'
        }).subscribe(() => this.loadSchedule());
      }
    });
  }

  cancelAppointment(appointment: DoctorAppointmentResponseDto) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Cancel Appointment',
        message: `Are you sure you want to cancel your appointment with ${appointment.patientName}?`,
        confirmText: 'Yes, Cancel it',
        cancelText: 'Keep Appointment'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.appointmentService.cancelAppointment(appointment.appointmentId, 'Cancelled by doctor')
          .subscribe(() => this.loadSchedule());
      }
    });
  }

  getStatusBgColor(status: string) {
    switch(status.toLowerCase()) {
      case 'accepted': return '#dcfce7'; 
      case 'pending': return '#fef3c7'; 
      case 'rescheduled': return '#dbeafe'; 
      default: return '#f3f4f6'; 
    }
  }

  getStatusBorderColor(status: string) {
    switch(status.toLowerCase()) {
      case 'accepted': return '#22c55e'; 
      case 'pending': return '#f59e0b'; 
      case 'rescheduled': return '#3b82f6'; 
      default: return '#e5e7eb'; 
    }
  }

  getStatusTextColor(status: string) {
    switch(status.toLowerCase()) {
      case 'accepted': return '#14532d'; 
      case 'pending': return '#78350f'; 
      case 'rescheduled': return '#1e3a8a'; 
      default: return '#374151'; 
    }
  }
}