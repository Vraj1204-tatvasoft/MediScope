// src/app/core/models/document.model.ts

export interface PatientDocumentResponseDto {
    id: string;
    fileName: string;
    description: string;
    category: string;
    doctorName: string;
    isViewedByDoctor: boolean;
    isReviewed: boolean;
    feedback: string | null;
    severity: string | null;
    uploadedAt: string;
    reviewedAt: string | null;
  }

  export interface DoctorDocumentResponseDto {
    id: string;
    patientId: string;
    patientName: string;
    fileName: string;
    category: string;
    description: string;
    uploadedAt: string;
    isViewedByDoctor: boolean;
    isReviewed: boolean;
    feedback: string | null;
    severity: string | null;
  }