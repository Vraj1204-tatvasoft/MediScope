import { Component, Inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { QuestionItem, FIELD_TYPES, CHOICE_FIELD_TYPES } from '../../../models/questionnaire.model';
import { QuestionnaireService } from '../../../services/questionnaire.service';

export interface QuestionModalData {
  questionnaireId: string;
  questionnaireName: string;
  question?: QuestionItem;
  nextDisplayOrder?: number;
}

@Component({
  selector: 'app-question-form-modal',
  standalone: true,
  imports: [ReactiveFormsModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './question-form-modal.component.html',
  styleUrls: ['./question-form-modal.component.css']
})
export class QuestionFormModalComponent {
  form: FormGroup;
  isEditMode = false;
  title = 'Add Question';
  submitLabel = 'Add Question';
  
  fieldTypes = FIELD_TYPES;
  
  saving = signal(false);
  newOptionText = signal('');
  errorMsg = signal('');

  get isChoiceType(): boolean {
    return CHOICE_FIELD_TYPES.includes(this.form.get('fieldType')?.value);
  }

  get options(): FormArray {
    return this.form.get('options') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<QuestionFormModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: QuestionModalData,
    private questionnaireService: QuestionnaireService
  ) {
    this.isEditMode = !!data.question;
    this.title = this.isEditMode ? 'Edit Question' : 'Add Question';
    this.submitLabel = this.isEditMode ? 'Save Changes' : 'Add Question';

    this.form = this.fb.group({
      label: [data.question?.label || '', Validators.required],
      fieldType: [data.question?.fieldType || 'TextBox', Validators.required],
      placeholder: [data.question?.placeholder || ''],
      isRequired: [data.question?.isRequired ?? false],
      defaultValue: [data.question?.defaultValue || ''],
      
      // New Validation Fields
      minValue: [data.question?.minValue ?? null],
      maxValue: [data.question?.maxValue ?? null],
      minLength: [data.question?.minLength ?? null],
      maxLength: [data.question?.maxLength ?? null],
      regexPattern: [data.question?.regexPattern || ''],

      options: this.fb.array([])
    });

    if (this.isEditMode && data.question?.options) {
      data.question.options.forEach(opt => {
        this.options.push(this.fb.group({
          label: [opt.label, Validators.required],
          value: [opt.value, Validators.required]
        }));
      });
    }
  }

  hasError(controlName: string, errorName: string) {
    return this.form.get(controlName)?.hasError(errorName) && this.form.get(controlName)?.touched;
  }

  optionAt(index: number): FormGroup {
    return this.options.at(index) as FormGroup;
  }

  addOption() {
    const text = this.newOptionText().trim();
    if (!text) return;

    this.options.push(this.fb.group({
      label: [text, Validators.required],
      value: [text, Validators.required] 
    }));
    this.newOptionText.set('');
    this.errorMsg.set('');
  }

  addOptionOnEnter(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.addOption();
    }
  }

  removeOption(index: number) {
    this.options.removeAt(index);
  }

  cancel() {
    this.dialogRef.close();
  }

  submit() {
    this.form.markAllAsTouched();
    this.errorMsg.set('');

    if (this.form.invalid) return;

    if (this.isChoiceType && this.options.length < 2) {
      this.errorMsg.set('Choice fields require at least 2 options.');
      return;
    }

    this.saving.set(true);
    const formValue = this.form.value;

    const payload = {
      label: formValue.label,
      fieldType: formValue.fieldType,
      placeholder: this.isChoiceType ? null : formValue.placeholder,
      isRequired: formValue.isRequired,
      displayOrder: this.isEditMode ? (this.data.question?.displayOrder || 0) : (this.data.nextDisplayOrder || 0),
      defaultValue: this.isChoiceType ? null : formValue.defaultValue,
      
      // Conditionally attach validation data based on field type
      minValue: formValue.fieldType === 'Number' ? formValue.minValue : null,
      maxValue: formValue.fieldType === 'Number' ? formValue.maxValue : null,
      minLength: ['TextBox', 'TextArea'].includes(formValue.fieldType) ? formValue.minLength : null,
      maxLength: ['TextBox', 'TextArea'].includes(formValue.fieldType) ? formValue.maxLength : null,
      regexPattern: ['TextBox', 'TextArea'].includes(formValue.fieldType) ? formValue.regexPattern : null,

      options: this.isChoiceType 
        ? formValue.options.map((opt: any, i: number) => ({ ...opt, order: i })) 
        : null
    };

    if (this.isEditMode && this.data.question) {
      this.questionnaireService.updateQuestion(this.data.question.id, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.dialogRef.close(true);
        },
        error: () => {
          this.saving.set(false);
        }
      });
    } else {
      this.questionnaireService.addQuestion(this.data.questionnaireId, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.dialogRef.close(true);
        },
        error: () => {
          this.saving.set(false);
        }
      });
    }
  }
}