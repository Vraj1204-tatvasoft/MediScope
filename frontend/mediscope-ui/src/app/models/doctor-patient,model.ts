
export type ConnectionStatus =
  | 'pending_admin'    // patient submitted, waiting admin review
  | 'pending_doctor'   // admin approved, waiting doctor acceptance
  | 'active'           // fully connected
  | 'declined_doctor'  // doctor declined
  | 'rejected_admin'   // admin rejected
  | 'revoked';         // patient revoked

export interface PatientDoctorResponseDto {
  doctorPatientId:  string;
  doctorId:         string | null;   // null if patient didn't select a doctor
  patientId:        string;
  fullName:         string | null;   // null if doctor not yet assigned
  specialization:   string | null;
  hospital:         string | null;
  email:            string | null;
  contactNumber:    string | null;
  yearsExperience:  number | null;
  totalPatients:    number;
  status:           ConnectionStatus;
 // statusLabel:      string;          // friendly label from backend
  adminNote:        string | null;
  patientNote:      string | null;
  requestedAt:      string;
  assignedAt:       string | null;
}

export interface DoctorPatientResponseDto {
  doctorPatientId: string;
  doctorId:        string;
  patientId:       string;
  fullName:        string;
  email:           string;
  contactNumber:   string | null;
  gender:          string | null;
  bloodGroup:      string | null;
  dateOfBirth:     string | null;
  age:             number | null;
  patientNote:     string | null;
  status:          ConnectionStatus;
  requestedAt:     string;
  assignedAt:      string | null;
}

export interface RespondToRequestDto {
  doctorPatientId: string;
  accept:          boolean;
}

// ── Admin DTOs ──────────────────────────────────────────────
export interface AdminConnectionRequestDto {
  doctorPatientId:  string;
 // requestNumber:    string;
  patientId:        string;
  patientName:      string;
 // patientNote:      string | null;
  doctorId:         string | null;
  doctorName:       string | null;
  specialization:   string | null;
  status:           ConnectionStatus;
  adminNote:        string | null;
  requestedAt:      string;
  adminReviewedAt:  string | null;
}

export interface AdminApproveRequestDto {
  doctorPatientId: string;
  doctorId:        string;
  adminNote?:      string;
}

export interface AdminRejectRequestDto {
  doctorPatientId: string;
  adminNote?:      string;
}

export interface AdminDoctorPatientFilterDto {
  search?:   string;
  doctorId?: string;
  status?:   string;
}

export interface SendDoctorRequestDto {
  doctorId?:   string | null;   // optional — patient can request without selecting
  patientNote?: string;
}