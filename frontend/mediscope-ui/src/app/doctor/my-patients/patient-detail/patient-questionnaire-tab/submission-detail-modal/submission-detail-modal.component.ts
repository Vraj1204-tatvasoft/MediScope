import {
  Component, inject, signal, OnInit, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { SubmissionDetail } from '../../../../../models/questionnaire.model';
import { QuestionnaireService } from '../../../../../services/questionnaire.service';


export interface SubmissionDetailModalData {
  submissionId: string;
}

@Component({
  selector: 'app-submission-detail-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './submission-detail-modal.component.html',
  styleUrls: ['./submission-detail-modal.component.css'],
})
export class SubmissionDetailModalComponent implements OnInit {
  readonly data      = inject<SubmissionDetailModalData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<SubmissionDetailModalComponent>);
  private readonly svc = inject(QuestionnaireService);

  loading    = signal(false);
  submission = signal<SubmissionDetail | null>(null);
  errorMsg   = signal<string | null>(null);

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.svc.getSubmissionDetail(this.data.submissionId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: res => this.submission.set(res.data ?? null),
        error: () => this.errorMsg.set('Failed to load submission details.'),
      });
  }

  getAnswer(r: any): string {
    if (r.fieldType === 'Checkbox') {
      return r.responseValues?.length ? r.responseValues.join(', ') : '—';
    }
    return r.responseValue || '—';
  }

  isCheckbox(fieldType: string): boolean { return fieldType === 'Checkbox'; }

  fieldTypeLabel(ft: string): string {
    const map: Record<string, string> = {
      TextBox: 'Text', TextArea: 'Text Area', Number: 'Number',
      Date: 'Date', Dropdown: 'Dropdown', RadioButton: 'Radio', Checkbox: 'Checkbox',
    };
    return map[ft] ?? ft;
  }

  close(): void { this.dialogRef.close(); }
}
