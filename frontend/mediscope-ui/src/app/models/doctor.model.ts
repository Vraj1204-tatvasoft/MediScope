// ── File: src/app/core/models/doctor.models.ts ──────────────────────────────

export interface DoctorProfile {
    doctorId:        string;
    userId:          string;
    fullName:        string;
    email:           string;
    contactNumber:   string | null;
    specialization:  string | null;
    licenseNumber:   string;
    hospital:        string | null;
    yearsExperience: number | null;
    bio:             string | null;
    isActive:        boolean;
    assignedPatients: number;
    registeredAt:    string;
  }
  
  export interface CreateDoctorRequest {
    fullName:        string;
    email:           string;
    contactNumber:   string | null;
    specialization:  string;
    licenseNumber:   string;
    hospital:        string | null;
    yearsExperience: number | null;
    bio:             string | null;
  }
  
  export interface UpdateDoctorRequest {
    fullName:        string;
    contactNumber:   string | null;
    specialization:  string | null;
    hospital:        string | null;
    yearsExperience: number | null;
    bio:             string | null;
  }
  
  export const SPECIALIZATIONS = [
    'Cardiologist', 'Dermatologist', 'Endocrinologist',
    'General Physician', 'Gynecologist', 'Neurologist',
    'Oncologist', 'Ophthalmologist', 'Orthopedist',
    'Pediatrician', 'Psychiatrist', 'Pulmonologist',
    'Radiologist', 'Surgeon', 'Urologist'
  ] as const;
  