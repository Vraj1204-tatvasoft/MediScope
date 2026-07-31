import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDivider } from '@angular/material/divider';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, debounceTime, distinctUntilChanged } from 'rxjs';

import { QuestionnaireService } from '../../services/questionnaire.service';
import { QuestionnaireFormModalComponent } from '../question-modals/questionnaire-form-modal/questionnaire-form-modal.component';
import { ConfirmDeleteModalComponent, ConfirmDeleteModalData } from '../question-modals/confirm-delete-modal/confirm-delete-modal.component';
import { QuestionnaireListItem } from '../../models/questionnaire.model';

@Component({
  selector: 'app-admin-questionnaire',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule, // Added for search control
    MatIconModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatTooltipModule,
    MatDivider
  ],
  templateUrl: './admin-questionnaire.component.html',
  styleUrls: ['./admin-questionnaire.component.css']
})
export class AdminQuestionnaireComponent implements OnInit {
  private readonly svc = inject(QuestionnaireService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  questionnaires = signal<QuestionnaireListItem[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  activeCount = signal(0);
  togglingId = signal<string | null>(null);

  // Search Signals and Controls
  searchControl = new FormControl('');
  searchTerm = signal<string>('');
  
  // Computed signal to filter items locally based on search term
  filteredQuestionnaires = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.questionnaires();
    return this.questionnaires().filter(q => q.name.toLowerCase().includes(term));
  });

  constructor() {
    // Listen to search input changes, debounce by 300ms, and update the signal
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed() // Automatically unsubscribes when component is destroyed
    ).subscribe(val => {
      this.searchTerm.set(val || '');
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    
    this.svc.getQuestionnaires({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        const items = res.data?.items ?? [];
        this.questionnaires.set(items);
        this.totalCount.set(res.data?.totalCount ?? 0);
        
        // Calculate active count locally
        this.activeCount.set(items.filter(q => q.status === 'Active').length);
        this.loading.set(false);
      },
      error: () => {
        // Interceptor handles the toast, just turn off the spinner
        this.loading.set(false);
      }
    });
  }

  openCreateModal(): void {
    const ref = this.dialog.open<QuestionnaireFormModalComponent>(
      QuestionnaireFormModalComponent,
      { 
        width: '560px', 
        disableClose: true 
      }
    );
    
    ref.afterClosed().subscribe(saved => {
      if (saved) this.load();
    });
  }

  openEditModal(q: QuestionnaireListItem): void {
    const ref = this.dialog.open<QuestionnaireFormModalComponent>(
      QuestionnaireFormModalComponent,
      {
        width: '560px',
        data: q, // Pass the questionnaire data directly
        disableClose: true
      }
    );
    
    ref.afterClosed().subscribe(saved => {
      if (saved) this.load();
    });
  }

  toggleStatus(q: QuestionnaireListItem): void {
    this.togglingId.set(q.id);
    
    this.svc.toggleStatus(q.id)
      .pipe(finalize(() => this.togglingId.set(null)))
      .subscribe({
        next: () => this.load()
      });
  }

  openDeleteModal(q: QuestionnaireListItem): void {
    const ref = this.dialog.open<ConfirmDeleteModalComponent, ConfirmDeleteModalData>(
      ConfirmDeleteModalComponent,
      {
        width: '420px',
        data: {
          title: 'Delete Questionnaire',
          message: `Are you sure you want to delete "${q.name}"? This action cannot be undone.`
        }
      }
    );

    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      
      this.svc.deleteQuestionnaire(q.id).subscribe({
        next: () => this.load()
      });
    });
  }

  goToQuestions(q: QuestionnaireListItem): void {
    this.router.navigate(['/admin/admin-questions', q.id], {
      state: { questionnaireName: q.name }
    });
  }

  trackById(index: number, q: QuestionnaireListItem): string {
    return q.id;
  }
}