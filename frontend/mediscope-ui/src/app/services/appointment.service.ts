import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { BaseHttpService } from './base-http.service'; 
import { ApiResponse } from '../models/api-response.model';
import { 
  DoctorAppointmentResponseDto, 
  CreateAppointmentRequestDto, 
  RescheduleAppointmentRequestDto, 
  RespondToAppointmentRequestDto,
  PatientDto,
  PatientAppointmentResponseDto
} from '../models/appointment.model';
import { AppointmentDto } from '../models/invoice.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  
  private baseEndpoint = 'appointments';
  private userEndpoint = 'doctor-patient';

  constructor(private http: BaseHttpService) {}

  getDoctorSchedule(): Observable<ApiResponse<DoctorAppointmentResponseDto[]>> {
    return this.http.get<DoctorAppointmentResponseDto[]>(`${this.baseEndpoint}/doctor/my-schedule`,{
        showError: true,
        showSuccess: false
      });
  }

  getDoctorPatients(): Observable<ApiResponse<PatientDto[]>> {
    return this.http.get<PatientDto[]>(`${this.userEndpoint}/my-patients`,{
        showError: true,
        showSuccess: false
      });
  }

  getPatientAppointments(): Observable<ApiResponse<PatientAppointmentResponseDto[]>> {
    return this.http.get<PatientAppointmentResponseDto[]>(`${this.baseEndpoint}/patient/my-appointments`,{
        showError: true,
        showSuccess: false
      });
  }

  createAppointment(request: CreateAppointmentRequestDto): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.baseEndpoint}`, request, { 
      showSuccess: true,
      showError: true,
    });
  }

  rescheduleAppointment(id: string, request: RescheduleAppointmentRequestDto): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.baseEndpoint}/${id}/reschedule`, request, { 
      showSuccess: true,
      showError: true,
    });
  }

  respondToAppointment(id: string, request: RespondToAppointmentRequestDto): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.baseEndpoint}/${id}/respond`, request, { 
      showSuccess: true,
      showError:true,
    });
  }

  cancelAppointment(id: string, reason?: string): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.baseEndpoint}/${id}/cancel`, { reason }, { 
      showSuccess: true,
      showError: true,
    });
  }
  
  completeAppointment(id: string): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.baseEndpoint}/${id}/complete`, {}, { 
      showSuccess: true 
    });
  }

  getAppointmentsByPatient(patientId: string): Observable<AppointmentDto[]> {
    return this.http.get<AppointmentDto[]>(`appointments/patient/${patientId}`).pipe(
      map(res => res.data ? res.data : (Array.isArray(res) ? res : []))
    );
  }
}