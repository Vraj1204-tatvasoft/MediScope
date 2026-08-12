import {
  Component, Input, OnInit, signal, computed,
  inject, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router'; 
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';

import { QuestionnaireService } from '../../../../services/questionnaire.service';
import { NotificationService }  from '../../../../core/services/notification.service';
import { PatientAssignmentResponseDto } from '../../../../models/questionnaire.model';

import {
  ConfirmDeleteModalComponent,
  ConfirmDeleteModalData,
} from '../../../../admin/question-modals/confirm-delete-modal/confirm-delete-modal.component';

import {
  AssignQuestionnaireModalComponent,
  AssignQuestionnaireModalData,
} from './assign-questionnaire-modal/assign-questionnaire-modal.component';

import {
  SubmissionDetailModalComponent,
  SubmissionDetailModalData,
} from './submission-detail-modal/submission-detail-modal.component';

import {
  QuestionnairePreviewModalComponent,
  QuestionnairePreviewModalData,
} from './questionnaire-preview-modal/questionnaire-preview-modal.component';

@Component({
  selector: 'app-patient-questionnaire-tab',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './patient-questionnaire-tab.component.html',
  styleUrls: ['./patient-questionnaire-tab.component.css'],
})
export class PatientQuestionnaireTabComponent implements OnInit {
  @Input({ required: true }) patientId!: string;
  @Input() patientName = '';

  private readonly svc    = inject(QuestionnaireService);
  private readonly dialog = inject(MatDialog);
  private readonly notify = inject(NotificationService);
  private readonly route  = inject(ActivatedRoute); 
  private readonly router = inject(Router);         

  private hasAutoOpened = false; 
  assignments  = signal<PatientAssignmentResponseDto[]>([]);
  loading      = signal(false);
  removingId   = signal<string | null>(null);

  totalCount     = computed(() => this.assignments().length);
  pendingCount   = computed(() => this.assignments().filter(a => a.fillStatus === 'Pending').length);
  submittedCount = computed(() => this.assignments().filter(a => a.fillStatus === 'Submitted').length);
  draftCount     = computed(() => this.assignments().filter(a => a.fillStatus === 'Draft').length);

  // ── Lifecycle 
  ngOnInit(): void { 
    this.load(); 
  }

  // ── Load 
  private load(): void {
    this.loading.set(true);
    this.svc.getPatientAssignments(this.patientId, { pageNumber: 1, pageSize: 100 })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: res => {
          const items = res.data?.items ?? [];
          this.assignments.set(items);

          this.checkAndOpenFromQuery(items);
        },
        error: () => this.notify.error('Failed to load questionnaire assignments.'),
      });
  }

  // ── Auto-Open Modal Logic ──────────────────────────────────────────────────
  private checkAndOpenFromQuery(items: PatientAssignmentResponseDto[]): void {
    if (this.hasAutoOpened) return;

    const openSubmissionId = this.route.snapshot.queryParamMap.get('openSubmissionId');
    if (!openSubmissionId) return;

    const targetAssignment = items.find(a => a.submissionId === openSubmissionId);
    if (targetAssignment) {
      this.hasAutoOpened = true;
      this.viewSubmission(targetAssignment);

      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { openSubmissionId: null },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    }
  }

  // ── Assign ─────────────────────────────────────────────────────────────────
  openAssignModal(): void {
    const ref = this.dialog.open<AssignQuestionnaireModalComponent, AssignQuestionnaireModalData>(
      AssignQuestionnaireModalComponent,
      {
        width: '500px',
        data: { patientId: this.patientId, patientName: this.patientName },
        disableClose: true,
      }
    );
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  previewQuestions(a: PatientAssignmentResponseDto): void {
    this.dialog.open<QuestionnairePreviewModalComponent, QuestionnairePreviewModalData>(
      QuestionnairePreviewModalComponent,
      {
        width: '760px',
        maxHeight: '92vh',
        data: {
          questionnaireId:   a.questionnaireId,
          questionnaireName: a.questionnaireName,
        },
        panelClass: 'preview-dialog-panel',
      }
    );
  }

  // ── Remove ─────────────────────────────────────────────────────────────────
  openRemoveModal(a: PatientAssignmentResponseDto): void {
    const ref = this.dialog.open<ConfirmDeleteModalComponent, ConfirmDeleteModalData>(
      ConfirmDeleteModalComponent,
      {
        width: '420px',
        data: {
          title: 'Remove Assignment',
          message: `This will remove the assignment of "${a.questionnaireName}" from this patient. This cannot be undone.`,
        },
      }
    );
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.removingId.set(a.assignmentId);
      this.svc.unassignQuestionnaire(a.assignmentId)
        .pipe(finalize(() => this.removingId.set(null)))
        .subscribe({
          next: () => { this.notify.success('Assignment removed.'); this.load(); },
          error: () => this.notify.error('Failed to remove assignment.'),
        });
    });
  }

  // ── View Submitted Response ────────────────────────────────────────────────
  viewSubmission(a: PatientAssignmentResponseDto): void {
    if (!a.submissionId) return;
    this.dialog.open<SubmissionDetailModalComponent, SubmissionDetailModalData>(
      SubmissionDetailModalComponent,
      {
        width: '760px',
        maxHeight: '92vh',
        data: {
          submissionId:      a.submissionId,
          assignmentId:      a.assignmentId,
          questionnaireId:   a.questionnaireId,
          questionnaireName: a.questionnaireName,
        },
        panelClass: 'preview-dialog-panel',
      }
    );
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  statusClass(status: string): string {
    return ({ Pending: 'status-pending', Draft: 'status-draft', Submitted: 'status-submitted' } as any)[status]
      ?? 'status-pending';
  }

  statusIcon(status: string): string {
    return ({ Pending: 'schedule', Draft: 'edit_note', Submitted: 'check_circle' } as any)[status]
      ?? 'schedule';
  }

  trackById(_: number, a: PatientAssignmentResponseDto): string { return a.assignmentId; }
}