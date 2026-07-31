import {
  Component, inject, signal, OnInit, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';

import {
  SubmissionDetail,
  SubmissionResponseItem,
  SubmissionOptionItem,
  FieldType,
} from '../../../../../models/questionnaire.model';
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

  // ── Field type helpers ──────────────────────────────────────────────────────

  fieldTypeLabel(ft: FieldType | string): string {
    const map: Record<string, string> = {
      TextBox: 'Text', TextArea: 'Text Area', Number: 'Number',
      Date: 'Date', Dropdown: 'Dropdown', RadioButton: 'Radio', Checkbox: 'Checkbox',
    };
    return map[ft] ?? ft;
  }

  isText(ft: string): boolean     { return ft === 'TextBox' || ft === 'TextArea'; }
  isNumber(ft: string): boolean   { return ft === 'Number'; }
  isDate(ft: string): boolean     { return ft === 'Date'; }
  isDropdown(ft: string): boolean { return ft === 'Dropdown'; }
  isRadio(ft: string): boolean    { return ft === 'RadioButton'; }
  isCheckbox(ft: string): boolean { return ft === 'Checkbox'; }

  hasAnswer(r: SubmissionResponseItem): boolean {
    if (r.fieldType === 'Checkbox') return !!(r.responseValues?.length);
    return !!(r.responseValue?.trim());
  }

  // ── Selection helpers (used in template) ───────────────────────────────────

  /** Is this option the selected one? (Radio / Dropdown) */
  isSelected(opt: SubmissionOptionItem, r: SubmissionResponseItem): boolean {
    return opt.value === r.responseValue;
  }

  /** Is this checkbox option ticked? */
  isChecked(opt: SubmissionOptionItem, r: SubmissionResponseItem): boolean {
    return r.responseValues?.includes(opt.value) ?? false;
  }

  close(): void { this.dialogRef.close(); }
}