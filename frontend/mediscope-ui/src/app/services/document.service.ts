// src/app/core/services/document.service.ts

import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BaseHttpService } from './base-http.service';
import { ApiResponse } from '../models/api-response.model';
import { DoctorDocumentResponseDto, PatientDocumentResponseDto } from '../models/document.model';
import { environment } from '../../environments/environments';
import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
@Injectable({ providedIn: 'root' })
export class DocumentService {
  private http = inject(BaseHttpService);
  constructor(private httpClient: HttpClient) {}
  uploadDocument(doctorId: string, file: File, category: string, description: string): Observable<boolean> {
    const formData = new FormData();
    formData.append('DoctorId', doctorId);
    formData.append('File', file);
    
    if (category) formData.append('Category', category);
    if (description) formData.append('Description', description);

    // Note: Adjust 'documents' if your backend controller route differs
    return this.http
      .post<boolean>('documents', formData, { 
        showSuccess: true, 
        showError: true 
      })
      .pipe(map(r => r.success));
  }
  getMyDocuments(): Observable<PatientDocumentResponseDto[]> {
    return this.http
      .get<PatientDocumentResponseDto[]>('documents/my', { 
        showError: true 
      })
      .pipe(map(r => r.data));
  }
  getDoctorDocuments(): Observable<DoctorDocumentResponseDto[]> {
    return this.http
      .get<DoctorDocumentResponseDto[]>('documents/doctor', { 
        showError: true 
      })
      .pipe(map(r => r.data));
  }

  markAsViewed(documentId: string): Observable<boolean> {
    return this.http
      .post<boolean>(`documents/${documentId}/view`, {}, { 
        showError: false, 
        showSuccess: false // Silent background update
      })
      .pipe(map(r => r.success));
  }

  addFeedback(documentId: string, feedback: string, severity: string): Observable<boolean> {
    return this.http
      .post<boolean>('documents/feedback', { documentId, feedback, severity }, { 
        showSuccess: true, 
        showError: true 
      })
      .pipe(map(r => r.success));
  }
  downloadDocumentFile(documentId: string): Observable<Blob> {
    const url = `${environment.apiUrl}/documents/${documentId}/download`;
    
    return this.httpClient.get(url, {
      responseType: 'blob' // Tells Angular NOT to parse as JSON
    });
  }
}