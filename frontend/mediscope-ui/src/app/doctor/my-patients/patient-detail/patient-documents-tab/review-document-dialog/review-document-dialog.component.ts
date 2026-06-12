import { Component, Inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DocumentService } from '../../../../../services/document.service';

@Component({
  selector: 'app-review-document-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, 
    MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule
  ],
  templateUrl: 'review-document-dialog.component.html',
  styleUrl: 'review-document-dialog.component.css'
})
export class ReviewDocumentDialogComponent {
  reviewForm: FormGroup;
  isSubmitting = signal(false);

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    public dialogRef: MatDialogRef<ReviewDocumentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { documentId: string, fileName: string }
  ) {
    this.reviewForm = this.fb.group({
      severity: ['Normal', Validators.required],
      feedback: ['', Validators.required]
    });
  }

  submitReview(): void {
    if (this.reviewForm.invalid) return;
    this.isSubmitting.set(true);
    const { feedback, severity } = this.reviewForm.value;

    this.documentService.addFeedback(this.data.documentId, feedback, severity).subscribe({
      next: (success) => {
        this.isSubmitting.set(false);
        if (success) this.dialogRef.close(true);
      },
      error: () => this.isSubmitting.set(false)
    });
  }
}