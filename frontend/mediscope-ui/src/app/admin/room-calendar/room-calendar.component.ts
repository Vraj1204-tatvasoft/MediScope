import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CalendarOptions } from '@fullcalendar/core';
import timeGridPlugin from '@fullcalendar/timegrid';
import { AdmissionService } from '../../services/admission.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FullCalendarModule } from '@fullcalendar/angular';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
@Component({
  selector: 'app-room-calendar',
  templateUrl: './room-calendar.component.html',
  styleUrls: ['./room-calendar.component.css'],
  standalone: true,
  imports: [
    CommonModule,          
    FormsModule,           
    MatButtonToggleModule, 
    MatIconModule,         
    MatTableModule,        
    MatTooltipModule,      
    FullCalendarModule     
  ]
})
export class RoomCalendarComponent implements OnChanges {
  @Input() roomId!: string;
  @Input() roomNumber!: string;

  viewMode: 'calendar' | 'list' = 'calendar';
  admissions: any[] = []; 

  calendarOptions: CalendarOptions = {
    plugins: [timeGridPlugin],
    initialView: 'timeGridDay',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'timeGridWeek,timeGridDay'
    },
    allDaySlot: false,
    slotMinTime: '00:00:00',
    slotMaxTime: '24:00:00',
    events: [], 
  };

  constructor(private admissionService: AdmissionService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['roomId'] && this.roomId) {
      this.loadRoomSchedule();
    }
  }

  private loadRoomSchedule() {
    if (!this.roomId) return;

    this.admissionService.getActivePatients(this.roomId).subscribe({
      next: (response : any) => {
        const data = response.data || response;
        this.admissions = data || []; 

        const mappedEvents = this.admissions.map(adm => ({
          title: `${adm.patientName} (Bed ${adm.bedNumber})`,
          start: adm.admissionDate,
          end: adm.expectedDischargeDate || new Date(new Date().setHours(23, 59, 59)), 
          color: adm.status === 0 ? '#1976d2' : '#ff9800' 
        }));

        this.calendarOptions = {
          ...this.calendarOptions,
          events: mappedEvents
        };
      },
      error: (err) => {
        console.error('Failed to load room schedule:', err);
        this.admissions = [];
        this.calendarOptions = { ...this.calendarOptions, events: [] };
      }
    });
  }
}