import { Component, Input, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { DoctorDocumentResponseDto } from '../../../../models/document.model';
import { DocumentService } from '../../../../services/document.service';
import { ReviewDocumentDialogComponent } from './review-document-dialog/review-document-dialog.component';
import { MatDivider } from '@angular/material/divider';

@Component({
  selector: 'app-patient-documents-tab',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDivider],
  templateUrl: './patient-documents-tab.component.html',
  styleUrls: ['./patient-documents-tab.component.css']
})
export class PatientDocumentsTabComponent implements OnInit {
  @Input({ required: true }) patientId!: string;
  @Input() patientName: string = ''; // Fallback if backend ID mapping is missing

  private documentService = inject(DocumentService);
  private dialog = inject(MatDialog);

  documents = signal<DoctorDocumentResponseDto[]>([]);
  isLoading = signal(true);
  downloadingId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.isLoading.set(true);
    this.documentService.getDoctorDocuments().subscribe({
      next: (docs) => {
        // Filter out documents to only show ones belonging to this specific patient tab
        const filteredDocs = docs.filter(d => 
          d.patientId === this.patientId || d.patientName === this.patientName
        );
        this.documents.set(filteredDocs);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  viewDocument(doc: DoctorDocumentResponseDto): void {
    // 1. Mark as viewed securely in the background if it's the first time
    if (!doc.isViewedByDoctor) {
      this.documentService.markAsViewed(doc.id).subscribe({
        next: () => doc.isViewedByDoctor = true
      });
    }

    // 2. Download and open the file
    this.downloadingId.set(doc.id);
    this.documentService.downloadDocumentFile(doc.id).subscribe({
      next: (blob) => {
        const fileUrl = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = fileUrl;
        anchor.target = '_blank';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(fileUrl);
        this.downloadingId.set(null);
      },
      error: () => this.downloadingId.set(null)
    });
  }

  openReviewDialog(doc: DoctorDocumentResponseDto): void {
    const dialogRef = this.dialog.open(ReviewDocumentDialogComponent, {
      width: '500px',
      data: { documentId: doc.id, fileName: doc.fileName }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadDocuments(); // Reload to fetch the new feedback and status
    });
  }

  formatDate(dateStr: string): string {
    return dateStr ? dateStr.split('T')[0] : '—';
  }
}