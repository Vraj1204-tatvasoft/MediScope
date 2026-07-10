export interface DoctorAppointmentResponseDto {
    appointmentId: string;
    startTime: string; 
    endTime: string;
    durationMinutes: number;
    status: string; 
    patientId: string;
    patientName: string;
    doctorNotes?: string;
    patientNotes?: string;
    createdBy: string;
    rescheduleRequestedBy?: string;
    rescheduleReason?: string;
  }

  export interface PatientAppointmentResponseDto {
    appointmentId: string;
    startTime: string;
    endTime: string;
    durationMinutes: number;
    status: string;
    doctorId: string;
    doctorName: string;
    specialization?: string;
    hospital?: string;
    doctorNotes?: string;
    patientNotes?: string;
    rescheduledTo?: string;
    rescheduleReason?: string;
    createdAt: string;
    createdBy: string;
    rescheduleRequestedBy?: string;
  }
  
  export interface CreateAppointmentRequestDto {
    patientId: string;
    startTime: string;
    durationMinutes: number;
    doctorNotes?: string;
  }
  
  export interface RescheduleAppointmentRequestDto {
    appointmentId: string;
    rescheduledTo: string;
    rescheduleReason?: string;
  }
  
  export interface RespondToAppointmentRequestDto {
    appointmentId: string;
    action: string;
    patientNotes?: string;
  }
  
  export interface PatientDto {
    patientId: string;
    fullName: string;
  }