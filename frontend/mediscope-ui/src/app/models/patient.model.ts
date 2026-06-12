// File: src/app/core/models/patient.models.ts

export interface PatientProfile {
    userId:               string;
    patientId:            string;
    fullName:             string;
    email:                string;
    contactNumber:        string | null;
    bloodGroup:           string | null;
    gender:               string | null;
    dateOfBirth:          string | null;
    address:              string | null;
    consentProfileVisible: boolean;
    registeredAt:         string;
    age:                  number | null;
  }
  
  export interface UpdateProfileRequest {
    fullName:      string;
    email:         string;
    contactNumber: string | null;
    bloodGroup:    string | null;
    gender:        string | null;
    dateOfBirth:   string | null;
    address:       string | null;
  }
  
  export interface ChangePasswordRequest {
    currentPassword:    string;
    newPassword:        string;
    confirmNewPassword: string;
  }
  
  export const BLOOD_GROUPS = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'] as const;
  export const GENDERS      = ['Male', 'Female', 'Other', 'PreferNotToSay']        as const;