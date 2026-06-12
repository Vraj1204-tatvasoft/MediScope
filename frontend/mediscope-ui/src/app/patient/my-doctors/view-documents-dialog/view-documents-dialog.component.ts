import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PatientDocumentResponseDto } from '../../../models/document.model';
import { DocumentService } from '../../../services/document.service';
import { NotificationService } from '../../../core/services/notification.service';

export interface ViewDocumentsDialogData {
  doctorId: string;
  doctorName: string;
}

@Component({
  selector: 'app-view-documents-dialog',
  standalone: true,
  imports: [
    CommonModule, MatDialogModule, MatButtonModule, 
    MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './view-documents-dialog.component.html',
  styleUrls: ['./view-documents-dialog.component.css']
})
export class ViewDocumentsDialogComponent implements OnInit {
  isLoading = signal<boolean>(true);
  documents = signal<PatientDocumentResponseDto[]>([]);
  downloadingId = signal<string | null>(null);
  private notify = Inject(NotificationService);
  constructor(
    private documentService: DocumentService,
    public dialogRef: MatDialogRef<ViewDocumentsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ViewDocumentsDialogData
  ) {}

  ngOnInit(): void {
    this.fetchDocuments();
  }

  fetchDocuments(): void {
    this.isLoading.set(true);
    
    // Call the /my endpoint
    this.documentService.getMyDocuments().subscribe({
      next: (allDocs) => {
        // Only keep documents matching this dialog's doctor 
        const filteredDocs = (allDocs || []).filter(
          doc => doc.doctorName === this.data.doctorName
        );
        
        this.documents.set(filteredDocs);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
  viewFile(docId: string, fileName: string): void {
    this.downloadingId.set(docId);
    
    this.documentService.downloadDocumentFile(docId).subscribe({
      next: (blob: Blob) => {
        // 1. Create a local URL pointing to the binary data in the browser's memory
        const fileUrl = window.URL.createObjectURL(blob);
        
        // 2. Create an invisible anchor tag to trigger the opening
        const anchor = document.createElement('a');
        anchor.href = fileUrl;
        
        // anchor.download = fileName; 
        
        anchor.target = '_blank';
        document.body.appendChild(anchor);
        anchor.click();
        
        // 3. Clean up
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(fileUrl);
        
        this.downloadingId.set(null);
      },
      error: () => {
        this.downloadingId.set(null);
        this.notify.error('Unable to retrieve the file. It may have been moved or deleted.');
      }
    });
  }
  formatDate(dateStr: string | null): string {
    if (!dateStr) return '—';
    return dateStr.split('T')[0];
  }

  getSeverityClass(severity: string | null): string {
    if (!severity) return 'normal';
    const s = severity.toLowerCase();
    if (s.includes('critical')) return 'critical';
    if (s.includes('elevated') || s.includes('warning')) return 'warning';
    return 'normal';
  }
}