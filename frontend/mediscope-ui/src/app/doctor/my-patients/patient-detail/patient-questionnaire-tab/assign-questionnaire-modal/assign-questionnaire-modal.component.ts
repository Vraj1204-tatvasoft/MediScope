import {
  Component, inject, signal, OnInit, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { ActiveQuestionnaire } from '../../../../../models/questionnaire.model';
import { QuestionnaireService } from '../../../../../services/questionnaire.service';
import { QuestionnairePreviewModalComponent } from '../questionnaire-preview-modal/questionnaire-preview-modal.component';
import { MatDialog } from '@angular/material/dialog';
export interface AssignQuestionnaireModalData {
  patientId: string;
  patientName: string;
}

@Component({
  selector: 'app-assign-questionnaire-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './assign-questionnaire-modal.component.html',
  styleUrls: ['./assign-questionnaire-modal.component.css'],
})
export class AssignQuestionnaireModalComponent implements OnInit {
  readonly data      = inject<AssignQuestionnaireModalData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<AssignQuestionnaireModalComponent>);
  private readonly svc = inject(QuestionnaireService);
  private readonly fb  = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  questionnaires  = signal<ActiveQuestionnaire[]>([]);
  loadingList     = signal(false);
  saving          = signal(false);
  errorMsg        = signal<string | null>(null);

  form!: FormGroup;

  ngOnInit(): void {
    this.form = this.fb.group({
      questionnaireId: ['', Validators.required],
      notes:           [''],
    });
    this.loadQuestionnaires();
  }

  private loadQuestionnaires(): void {
    this.loadingList.set(true);
    this.svc.getActiveQuestionnaires()
      .pipe(finalize(() => this.loadingList.set(false)))
      .subscribe({
        next: res => this.questionnaires.set(res.data ?? []),
        error: () => this.errorMsg.set('Failed to load questionnaires.'),
      });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.errorMsg.set(null);
    this.saving.set(true);

    this.svc.assignQuestionnaire({
      questionnaireId: this.form.value.questionnaireId,
      patientId:       this.data.patientId,
      notes:           this.form.value.notes?.trim() || null,
    })
    .pipe(finalize(() => this.saving.set(false)))
    .subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => this.errorMsg.set(err?.error?.message ?? 'Failed to assign questionnaire.'),
    });
  }

  cancel(): void { this.dialogRef.close(false); }

  hasError(field: string, error: string): boolean {
    const c = this.form.get(field);
    return !!(c?.touched && c?.hasError(error));
  }
  previewSelected() {
    const selectedId = this.form.get('questionnaireId')?.value;
    if (!selectedId) return;

    const selectedQ = this.questionnaires().find(q => q.id === selectedId);

    this.dialog.open(QuestionnairePreviewModalComponent, {
      width: '760px',
      maxHeight: '92vh',
      data: {
        questionnaireId: selectedId,
        questionnaireName: selectedQ?.name || 'Questionnaire Preview'
      }
    });
  }
}
