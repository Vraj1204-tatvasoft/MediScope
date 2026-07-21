import { Component, Input, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions, EventClickArg } from '@fullcalendar/core';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import { AdmissionService } from '../../services/admission.service';
import { RoomPatient } from '../../models/admission.model';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-room-calendar',
  standalone: true,
  imports: [CommonModule, FullCalendarModule, MatCardModule
],
  templateUrl: './room-calendar.component.html',
  styleUrls: ['./room-calendar.component.css']
})
export class RoomCalendarComponent implements OnChanges {

  @Input() roomId!: string;
  @Input() roomNumber = '';

  private admissionService = inject(AdmissionService);

  selectedPatient: RoomPatient | null = null;

  calendarOptions: CalendarOptions = {
    plugins: [timeGridPlugin, interactionPlugin],
    initialView: 'timeGridDay',
    allDaySlot: false,
    nowIndicator: true,
    editable: false,
    height: 500,
    slotMinTime: '00:00:00',
    slotMaxTime: '24:00:00',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'timeGridDay,timeGridWeek'
    },
    events: [],
    eventClick: this.onEventClick.bind(this)
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['roomId'] && this.roomId) {
      this.loadRoomSchedule();
    }
  }

  loadRoomSchedule(): void {
    this.admissionService.getActivePatients(this.roomId).subscribe({
      next: res => {
        const patients = res.data;

        this.calendarOptions = {
          ...this.calendarOptions,
          events: patients.map(p => ({
            id: p.admissionId,
            title: p.patientName,
            start: p.admissionDate,
            end: p.expectedDischargeDate,
            extendedProps: {
              patient: p
            }
          }))
        };
      }
    });
  }
  onEventClick(arg: EventClickArg): void {
    this.selectedPatient = arg.event.extendedProps['patient'];
  }

}