
import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { QuestionnaireRendererComponent } from '../../../../../patient/questionnaire-renderer/questionnaire-renderer.component';

export interface QuestionnairePreviewModalData {
  questionnaireId: string;
  questionnaireName: string;
}

@Component({
  selector: 'app-questionnaire-preview-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatDialogModule,
    QuestionnaireRendererComponent,
  ],
  templateUrl: `questionnaire-preview-modal.component.html`,
  styleUrls: ['questionnaire-preview-modal.component.css']
})
export class QuestionnairePreviewModalComponent {
  readonly data = inject<QuestionnairePreviewModalData>(MAT_DIALOG_DATA);
}