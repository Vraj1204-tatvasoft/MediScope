import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; 

// Angular Material Imports
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field'; 
import { MatInputModule } from '@angular/material/input';         
import { MatSelectModule } from '@angular/material/select';       

import { QuestionnaireService } from '../../services/questionnaire.service';
import { PatientService } from '../../services/patient.service';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

export interface AssignedQuestionnaire {
  id: string;
  questionnaireId: string;
  name: string;
  assignedBy: string;
  assignedDate: Date;
  status: 'Pending' | 'Draft' | 'Submitted';
  versionCount: number;
}

@Component({
  selector: 'app-patient-questionnaire-list',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule, 
    MatChipsModule, 
    MatCardModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatFormFieldModule, 
    MatInputModule,     
    MatSelectModule     
  ],
  templateUrl: './patient-questionnaire-list.component.html',
  styleUrls: ['./patient-questionnaire-list.component.css']
})
export class PatientQuestionnaireListComponent implements OnInit {
  private router = inject(Router);
  private service = inject(QuestionnaireService);
  private patientService = inject(PatientService);
  private searchSubject = new Subject<string>();
  patientId = ''; 

  loading = signal<boolean>(true);
  dataSource = signal<AssignedQuestionnaire[]>([]);
  displayedColumns: string[] = ['name', 'assignedBy', 'assignedDate', 'status', 'action'];

  // Pagination State
  totalItems = signal<number>(0);
  pageSize = signal<number>(10);
  pageIndex = signal<number>(0);

  // Filter State
  filterStatus = signal<string>('');
  filterAssignedBy = signal<string>('');

  ngOnInit(): void {
    this.patientService.getMyProfile().subscribe({
      next: (profile: any) => {
        this.patientId = profile.patientId; 

        if (this.patientId) {
          this.fetchQuestionnaires();
        } else {
          console.error('Patient profile loaded, but no ID was found.');
          this.loading.set(false);
        }
      },
      error: (err) => {
        console.error('Failed to load patient profile', err);
        this.loading.set(false);
      }
    });
    this.searchSubject.pipe(
      debounceTime(400), 
      distinctUntilChanged() 
    ).subscribe(searchValue => {
      this.filterAssignedBy.set(searchValue);
      this.applyFilters();
    });
  }

  fetchQuestionnaires(): void {
    this.loading.set(true);
    
    // Add the filters to your API payload
    const filter = {
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      status: this.filterStatus() || undefined,
      assignedBy: this.filterAssignedBy() || undefined
    };

    this.service.getPatientAssignments(this.patientId, filter).subscribe({
      next: (res) => {
        const mappedData: AssignedQuestionnaire[] = res.data.items.map((item: any) => ({
          id: item.assignmentId,
          questionnaireId: item.questionnaireId,
          name: item.questionnaireName,
          assignedBy: item.assignedByName || 'Clinical Staff',
          assignedDate: new Date(item.assignedAt), 
          status: item.fillStatus,
          versionCount: item.versionCount || 0
        }));
        
        this.dataSource.set(mappedData);
        this.totalItems.set(res.data.totalCount || 0); 
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load questionnaires', err);
        this.loading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.pageIndex.set(0); 
    this.fetchQuestionnaires();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.fetchQuestionnaires();
  }

  onActionClick(questionnaire: AssignedQuestionnaire): void {
    this.router.navigate(['/patient/questionnaire-renderer', questionnaire.id], {
      queryParams: { 
        patientId: this.patientId,
        questionnaireId: questionnaire.questionnaireId 
      }
    });
  }
  onSearchChange(value: string) {
    this.searchSubject.next(value);
  }
  ngOnDestroy() {
    this.searchSubject.complete();
  }
}