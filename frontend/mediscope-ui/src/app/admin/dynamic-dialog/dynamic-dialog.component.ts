import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
export interface FormFieldConfig {
  key: string;
  label: string;
  type: 'text' | 'number' | 'textarea' | 'select';
  value?: any;
  required?: boolean;
  options?: { label: string, value: any }[];
}

export interface DynamicDialogData {
  title: string;
  fields: FormFieldConfig[];
  onSubmitAsync?: (formData: any) => Observable<any>;
}

@Component({
  selector: 'app-dynamic-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, 
    MatFormFieldModule, MatInputModule, MatButtonModule, MatSelectModule
  ],
  templateUrl: './dynamic-dialog.component.html', 
  styles: ['.width-100 { width: 100%; } .mb-2 { margin-bottom: 8px; } .pt-2 { padding-top: 8px; }']
})
export class DynamicDialogComponent implements OnInit {
  data: DynamicDialogData = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<DynamicDialogComponent>);
  fb = inject(FormBuilder);
  
  form!: FormGroup;
  isSaving = false; 

  ngOnInit() {
    const group: any = {};
    for (const field of this.data.fields) {
      const validators = field.required ? [Validators.required] : [];
      group[field.key] = new FormControl(field.value || '', validators);
    }
    this.form = this.fb.group(group);
  }

  save() {
    if (this.form.invalid) return;

    if (this.data.onSubmitAsync) {
      this.isSaving = true;
      this.form.disable(); 

      this.data.onSubmitAsync(this.form.value).subscribe({
        next: () => {
          this.isSaving = false;
          this.dialogRef.close(true);
        },
        error: (err) => {
          this.isSaving = false;
          this.form.enable(); 
          console.error('Submission failed, dialog remains open:', err);
        }
      });
    } else {
      this.dialogRef.close(this.form.value);
    }
  }
}