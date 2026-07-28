import { Component, Inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { QuestionnaireDetail } from '../../../models/questionnaire.model';
import { QuestionnaireService } from '../../../services/questionnaire.service';


@Component({
  selector: 'app-questionnaire-form-modal',
  standalone: true,
  imports: [ReactiveFormsModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './questionnaire-form-modal.component.html',
  styleUrls: ['./questionnaire-form-modal.component.css']
})
export class QuestionnaireFormModalComponent {
  form: FormGroup;
  isEditMode = false;
  title = 'New Questionnaire';
  submitLabel = 'Create';
  saving = signal(false);
  
  // Adjust this list as needed for your hospital
  departments = ['General Medicine', 'Nursing', 'Cardiology', 'Neurology', 'Pediatrics'];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<QuestionnaireFormModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: QuestionnaireDetail | null,
    private questionnaireService: QuestionnaireService
  ) {
    this.isEditMode = !!data;
    this.title = this.isEditMode ? 'Edit Questionnaire' : 'New Questionnaire';
    this.submitLabel = this.isEditMode ? 'Save Changes' : 'Create';

    this.form = this.fb.group({
      name: [data?.name || '', [Validators.required, Validators.maxLength(255)]],
      description: [data?.description || '', Validators.required],
      department: [data?.department || ''],
      status: [data?.status || 'Active', Validators.required]
    });
  }

  hasError(controlName: string, errorName: string) {
    return this.form.get(controlName)?.hasError(errorName) && this.form.get(controlName)?.touched;
  }

  cancel() {
    this.dialogRef.close();
  }

  submit() {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.saving.set(true);
    const payload = this.form.value;

    // TypeScript Fix: Separate the subscriptions so the compiler knows the exact types
    if (this.isEditMode && this.data) {
      
      this.questionnaireService.updateQuestionnaire(this.data.id, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.dialogRef.close(true);
        },
        error: (err: any) => {
          this.saving.set(false);
        }
      });
      
    } else {
      
      this.questionnaireService.createQuestionnaire(payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.dialogRef.close(true);
        },
        error: (err: any) => {
          this.saving.set(false);
        }
      });
      
    }
  }
}