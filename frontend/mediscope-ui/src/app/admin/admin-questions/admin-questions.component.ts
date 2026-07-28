import {
  Component, inject, signal, computed, OnInit, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CdkDragDrop, CdkDrag, CdkDropList, CdkDragHandle, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { QuestionItem, FieldType } from '../../models/questionnaire.model';
import { QuestionnaireService } from '../../services/questionnaire.service';
import { QuestionFormModalComponent, QuestionModalData } from '../question-modals/question-form-modal/question-form-modal.component';
import { ConfirmDeleteModalComponent, ConfirmDeleteModalData } from '../question-modals/confirm-delete-modal/confirm-delete-modal.component';
import { MatMenu, MatMenuModule, MatMenuTrigger } from '@angular/material/menu';
import { MatDivider, MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-admin-questions',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    CdkDropList,
    CdkDrag,
    CdkDragHandle,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatMenu,
    MatDivider,
    MatMenuTrigger,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './admin-questions.component.html',
  styleUrls: ['./admin-questions.component.css'],
})
export class AdminQuestionsComponent implements OnInit {
  private readonly svc    = inject(QuestionnaireService);
  private readonly dialog = inject(MatDialog);
  private readonly route  = inject(ActivatedRoute);
  private readonly router = inject(Router);

  questions          = signal<QuestionItem[]>([]);
  loading            = signal(false);
  reordering         = signal(false);
  questionnaireId    = signal('');
  questionnaireName  = signal('');

  nextDisplayOrder = computed(() => this.questions().length + 1);

  ngOnInit(): void {
    const id   = this.route.snapshot.paramMap.get('id') ?? '';
    const name = (history.state as any)?.questionnaireName ?? 'Questionnaire';
    this.questionnaireId.set(id);
    this.questionnaireName.set(name);

    if (name === 'Questionnaire') {
      this.svc.getQuestionnaireById(id).subscribe({
        next: res => this.questionnaireName.set(res.data?.name ?? 'Questionnaire'),
      });
    }

    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.svc.getQuestions(this.questionnaireId())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: res => {
          const sorted = [...(res.data ?? [])].sort((a, b) => a.displayOrder - b.displayOrder);
          this.questions.set(sorted);
        }
      });
  }

  openAddModal(): void {
    const ref = this.dialog.open<QuestionFormModalComponent, QuestionModalData>(
      QuestionFormModalComponent,
      {
        width: '560px',
        data: {
          questionnaireId: this.questionnaireId(),
          questionnaireName: this.questionnaireName(),
          nextDisplayOrder: this.nextDisplayOrder(),
        },
        disableClose: true,
      }
    );
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  openEditModal(q: QuestionItem): void {
    const ref = this.dialog.open<QuestionFormModalComponent, QuestionModalData>(
      QuestionFormModalComponent,
      {
        width: '560px',
        data: {
          questionnaireId: this.questionnaireId(),
          questionnaireName: this.questionnaireName(),
          question: q,
        },
        disableClose: true,
      }
    );
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  openDeleteModal(q: QuestionItem): void {
    const ref = this.dialog.open<ConfirmDeleteModalComponent, ConfirmDeleteModalData>(
      ConfirmDeleteModalComponent,
      {
        width: '420px',
        data: {
          title: 'Delete Question',
          message: 'This will permanently delete this question and cannot be undone.',
        },
      }
    );
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.svc.deleteQuestion(q.id).subscribe({
        next: () => this.load()
      });
    });
  }

  onDrop(event: CdkDragDrop<QuestionItem[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const updated = [...this.questions()];
    moveItemInArray(updated, event.previousIndex, event.currentIndex);

    this.questions.set(updated);

    this.reordering.set(true);
    const orderMap = updated.map((q, i) => ({ id: q.id, order: i + 1 }));

    this.svc.reorderQuestions(this.questionnaireId(), { orderMap })
      .pipe(finalize(() => this.reordering.set(false)))
      .subscribe({
        error: () => {
          this.load();
        },
      });
  }

  moveUp(index: number): void {
    if (index === 0) return;
    const updated = [...this.questions()];
    moveItemInArray(updated, index, index - 1);
    this.questions.set(updated);
    this.persistOrder(updated);
  }

  moveDown(index: number): void {
    const list = this.questions();
    if (index === list.length - 1) return;
    const updated = [...list];
    moveItemInArray(updated, index, index + 1);
    this.questions.set(updated);
    this.persistOrder(updated);
  }

  private persistOrder(items: QuestionItem[]): void {
    this.reordering.set(true);
    const orderMap = items.map((q, i) => ({ id: q.id, order: i + 1 }));
    this.svc.reorderQuestions(this.questionnaireId(), { orderMap })
      .pipe(finalize(() => this.reordering.set(false)))
      .subscribe();
  }

  goBack(): void { 
    this.router.navigate(['/admin/admin-questionnaire']); 
  }

  fieldTypeLabel(ft: FieldType): string {
    const map: Record<FieldType, string> = {
      TextBox: 'Text Box', TextArea: 'Text Area', Number: 'Number',
      Date: 'Date', Dropdown: 'Dropdown', RadioButton: 'Radio Button', Checkbox: 'Checkbox',
    };
    return map[ft] ?? ft;
  }

  hasOptions(q: QuestionItem): boolean {
    return ['Dropdown', 'RadioButton', 'Checkbox'].includes(q.fieldType);
  }

  trackById(_: number, q: QuestionItem): string { return q.id; }
}