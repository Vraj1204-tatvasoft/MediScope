import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environments';
import { 
  DoctorAppointmentResponseDto, 
  CreateAppointmentRequestDto, 
  RescheduleAppointmentRequestDto, 
  RespondToAppointmentRequestDto,
  PatientDto,
  PatientAppointmentResponseDto
} from '../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = `${environment.apiUrl}/appointments`;
  private userApiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  getDoctorSchedule(): Observable<{ data: DoctorAppointmentResponseDto[] }> {
    return this.http.get<{ data: DoctorAppointmentResponseDto[] }>(`${this.apiUrl}/doctor/my-schedule`);
  }

  // Fetch dynamic patients connected to this doctor
  getDoctorPatients(): Observable<{ data: PatientDto[] }> {
    return this.http.get<{ data: PatientDto[] }>(`${this.userApiUrl}/doctor-patient/my-patients`);
  }

  createAppointment(request: CreateAppointmentRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}`, request);
  }

  rescheduleAppointment(id: string, request: RescheduleAppointmentRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/reschedule`, request);
  }

  respondToAppointment(id: string, request: RespondToAppointmentRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/respond`, request);
  }

  getPatientAppointments(): Observable<{ data: PatientAppointmentResponseDto[] }> {
    return this.http.get<{ data: PatientAppointmentResponseDto[] }>(`${this.apiUrl}/patient/my-appointments`);
  }
  
  cancelAppointment(id: string, reason?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/cancel`, { reason });
  }
}