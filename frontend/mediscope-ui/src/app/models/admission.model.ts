// Enums matching the C# backend
export enum AdmissionStatus {
    Active = 0,
    Discharged = 1,
    Scheduled = 2,
    Cancelled = 3
  }
  
  export interface AdmissionSummary {
    id: string;
    admissionNumber: string;
    patientName: string;
    doctorName: string;
    wardName: string;
    roomNumber: string;
    bedNumber: string;
    admissionDate: string;
    status: AdmissionStatus;
  }
  
  // Payload for Admission
  export interface AdmitPatientPayload {
    patientId: string;
    doctorId: string;
    wardId: string;
    roomId: string;
    bedId: string;
    admissionReason: string;
    admissionDate: string;
    expectedDischargeDate?: string | null; 
    remarks?: string | null;
  }
  
  // Payload for Transfer
  export interface TransferBedPayload {
    newWardId: string;
    newRoomId: string;
    newBedId: string;
    transferReason: string;
  }
  
  // Payload for Discharge
  export interface DischargePatientPayload {
    dischargeNotes: string;
    dischargeDate: string;
  }

  export interface AdmissionDetails {
    id: string;
    patientId: string;
    patientName: string;
    doctorId: string;
    wardId: string;
    wardName: string;
    roomId: string;
    roomNumber: string;
    bedId: string;
    bedNumber: string;
    admissionReason: string;
    admissionDate: string;
    expectedDischargeDate: string | null;
    remarks: string | null;
  }

  export interface RoomPatient {
    admissionId: string;
    patientId: string;
  
    patientName: string;
  
    doctorName: string;
  
    admissionDate: string;
  
    expectedDischargeDate: string | null;
  
    admissionReason: string;
  
    status: number;
  }

  export interface AvailableBedResponse {
    id: string;
    bedNumber: string;
  }