import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { QuestionnaireRendererComponent } from '../../../../../patient/questionnaire-renderer/questionnaire-renderer.component';

export interface SubmissionDetailModalData {
  submissionId: string;
  questionnaireId: string;
  questionnaireName?: string;
}
@Component({
  selector: 'app-submission-detail-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatDialogModule, QuestionnaireRendererComponent],
  templateUrl: './submission-detail-modal.component.html'
})
export class SubmissionDetailModalComponent {
  readonly data = inject<SubmissionDetailModalData>(MAT_DIALOG_DATA);
}