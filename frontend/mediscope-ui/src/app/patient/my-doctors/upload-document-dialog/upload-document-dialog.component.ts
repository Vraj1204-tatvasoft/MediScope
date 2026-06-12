import { Component, Inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DocumentService } from '../../../services/document.service';

export interface UploadDialogData {
  doctorId: string;
  doctorName: string;
}

@Component({
  selector: 'app-upload-document-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, 
    MatFormFieldModule, MatInputModule, MatSelectModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './upload-document-dialog.component.html',
  styleUrls: ['./upload-document-dialog.component.css']
})
export class UploadDocumentDialogComponent {
  uploadForm: FormGroup;
  selectedFile = signal<File | null>(null);
  isUploading = signal<boolean>(false);

  // Suggested predefined categories for structured medical routing
  documentCategories = ['Lab Result', 'Prescription', 'Medical Imaging', 'General Report', 'Other'];

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    public dialogRef: MatDialogRef<UploadDocumentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UploadDialogData
  ) {
    this.uploadForm = this.fb.group({
      category: ['', Validators.required],
      description: ['']
    });
  }

  onFileSelected(event: Event): void {
    const element = event.currentTarget as HTMLInputElement;
    const fileList: FileList | null = element.files;
    if (fileList && fileList.length > 0) {
      this.selectedFile.set(fileList[0]);
    } else {
      this.selectedFile.set(null);
    }
  }

  removeFile(): void {
    this.selectedFile.set(null);
  }

  onSubmit(): void {
    const file = this.selectedFile();
    if (this.uploadForm.invalid || !file) {
      this.uploadForm.markAllAsTouched();
      return;
    }

    this.isUploading.set(true);
    const formVals = this.uploadForm.value;

    this.documentService.uploadDocument(
      this.data.doctorId, 
      file, 
      formVals.category, 
      formVals.description
    ).subscribe({
      next: (success) => {
        this.isUploading.set(false);
        if (success) this.dialogRef.close(true);
      },
      error: () => {
        this.isUploading.set(false);
      }
    });
  }
}