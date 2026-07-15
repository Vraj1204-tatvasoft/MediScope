import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

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

  ngOnInit() {
    const group: any = {};
    for (const field of this.data.fields) {
      const validators = field.required ? [Validators.required] : [];
      group[field.key] = new FormControl(field.value || '', validators);
    }
    this.form = this.fb.group(group);
  }

  save() {
    if (this.form.valid) this.dialogRef.close(this.form.value);
  }
}