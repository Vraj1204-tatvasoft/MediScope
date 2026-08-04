import {
  Component, OnInit, Input, inject, signal,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder, FormGroup,
  FormArray, Validators, AbstractControl, ValidationErrors, FormsModule
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { MatCardModule }             from '@angular/material/card';
import { MatFormFieldModule }        from '@angular/material/form-field';
import { MatInputModule }            from '@angular/material/input';
import { MatSelectModule }           from '@angular/material/select';
import { MatRadioModule }            from '@angular/material/radio';
import { MatCheckboxModule }         from '@angular/material/checkbox';
import { MatDatepickerModule }       from '@angular/material/datepicker';
import { MatNativeDateModule }       from '@angular/material/core';
import { MatButtonModule }           from '@angular/material/button';
import { MatIconModule }             from '@angular/material/icon';
import { MatProgressSpinnerModule }  from '@angular/material/progress-spinner';
import { MatChipsModule }            from '@angular/material/chips';
import { MatDialogRef }              from '@angular/material/dialog';

import { QuestionnaireService } from '../../services/questionnaire.service';
import {
  RendererMode, QuestionItem, SubmissionDetail,
} from '../../models/questionnaire.model';
import { NotificationService }   from '../../core/services/notification.service';

@Component({
  selector: 'app-questionnaire-renderer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    FormsModule
  ],
  templateUrl: './questionnaire-renderer.component.html',
  styleUrls: ['./questionnaire-renderer.component.css'],
})
export class QuestionnaireRendererComponent implements OnInit {

  @Input() mode: RendererMode = 'fill';
  @Input() questionnaireId = '';
  @Input() questionnaireName = '';
  @Input() submissionId = '';
  @Input() assignmentId = '';
  private readonly fb      = inject(FormBuilder);
  private readonly route   = inject(ActivatedRoute);
  private readonly router  = inject(Router);
  private readonly svc     = inject(QuestionnaireService);
  private readonly notify  = inject(NotificationService);

  private dialogRef = (() => {
    try { return inject(MatDialogRef); } catch { return null; }
  })();

  private patientId    = '';

  questions    = signal<QuestionItem[]>([]);
  formStatus   = signal<string>('Pending');
  loading      = signal(true);
  savingDraft  = signal(false);
  submitting   = signal(false);
  isReadOnly   = signal(false);      
  submissionMeta = signal<SubmissionDetail | null>(null);

  versions = signal<any[]>([]);
  selectedSubmissionId = signal<string | null>(null);
  isCreatingNewVersion = signal<boolean>(false);

  form!: FormGroup;

  get isPreview(): boolean { return this.mode === 'preview'; }
  get isFill():    boolean { return this.mode === 'fill'; }
  get isView():    boolean { return this.mode === 'view'; }

  ngOnInit(): void {
    if (this.isPreview) {
      this.loadPreview();
    } else if (this.isView) {
      this.loadView();
    } else {
      this.loadFill();
    }
  }

  private loadView(): void {
    if (!this.questionnaireId || !this.submissionId || !this.assignmentId) {
      this.loading.set(false);
      return;
    }

    forkJoin({
      schema: this.svc.getQuestions(this.questionnaireId),
      versions: this.svc.getSubmissionVersions(this.assignmentId),
    }).subscribe({
      next: ({ schema, versions }) => {
    
        const qs = schema.data ?? [];
        const history = versions.data ?? [];
    
        const submittedVersions = history.filter(v => v.status === 'Submitted');
    
        if (!submittedVersions.length) {
          this.notify.error('No submitted response found.');
          this.loading.set(false);
          return;
        }
    
        // Latest submitted
        const latestSubmitted = submittedVersions.reduce((a, b) =>
          a.versionNumber > b.versionNumber ? a : b);
    
        this.versions.set(submittedVersions);
        this.selectedSubmissionId.set(latestSubmitted.submissionId);
    
        this.svc.getSubmissionDetail(latestSubmitted.submissionId)
          .subscribe(detailRes => {
    
            const detail = detailRes.data;
    
            this.questions.set(qs);
            this.submissionMeta.set(detail);
            this.formStatus.set(detail.status);
    
            this.buildForm(qs, detail.responses ?? []);
            this.form.disable();
            this.loading.set(false);
          });
      }
    });
}
  private loadPreview(): void {
    if (!this.questionnaireId) {
      this.loading.set(false);
      return;
    }

    this.svc.getQuestions(this.questionnaireId).subscribe({
      next: res => {
        const qs = res.data ?? [];
        this.questions.set(qs);
        this.isReadOnly.set(true);        
        this.buildForm(qs, []);           
        this.form.disable();
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load questionnaire questions.');
        this.loading.set(false);
      },
    });
  }

  private loadFill(): void {
    this.assignmentId   = this.route.snapshot.paramMap.get('assignmentId') || '';
    this.questionnaireId = this.route.snapshot.queryParamMap.get('questionnaireId') || '';
    this.patientId      = this.route.snapshot.queryParamMap.get('patientId') || '';

    if (!this.assignmentId || !this.questionnaireId || !this.patientId) {
      console.error('Missing assignmentId, questionnaireId, or patientId in route.');
      this.loading.set(false);
      return;
    }

    forkJoin({
      schema: this.svc.getQuestions(this.questionnaireId),
      draft:  this.svc.getRender(this.assignmentId, this.patientId).pipe(catchError(() => of({ data: null }))),
      versions: this.svc.getSubmissionVersions 
        ? this.svc.getSubmissionVersions(this.assignmentId).pipe(catchError(() => of({ data: [] })))
        : of({ data: [] })
    }).subscribe({
      next: ({ schema, draft, versions }) => {
        const questionList = schema.data ?? [];
        const draftData    = draft?.data;
        const vList        = versions?.data ?? [];

        this.questions.set(questionList);
        this.versions.set(vList);

        if (vList.length > 0) {
          if (draftData && draftData.status === 'Draft') {
            this.formStatus.set('Draft');
            this.isReadOnly.set(false);
            this.isCreatingNewVersion.set(true);
            this.buildForm(questionList, draftData.answers || []);
            this.loading.set(false);
          } else {
            const latest = vList.find((v: any) => v.isLatest) || vList[0];
            this.selectedSubmissionId.set(latest.submissionId);
            this.loadSubmissionDetail(latest.submissionId, questionList);
          }
        } else {
          this.formStatus.set(draftData?.status ?? 'Pending');
          this.isReadOnly.set(false);
          this.buildForm(questionList, draftData?.answers ?? []);
          this.loading.set(false);
        }
      },
      error: () => {
        this.notify.error('Failed to load questionnaire.');
        this.loading.set(false);
      },
    });
  }

  private loadSubmissionDetail(submissionId: string, questions?: QuestionItem[]) {
    const qs = questions || this.questions();
    this.loading.set(true);
    
    this.svc.getSubmissionDetail(submissionId).subscribe({
       next: (res) => {
          const detail = res.data;
          const status = detail?.status ?? 'Submitted';
          const isDraft = status === 'Draft'; 
          
          this.submissionMeta.set(detail);
          this.formStatus.set(status);
          this.isReadOnly.set(!isDraft);
          this.isCreatingNewVersion.set(isDraft);
          
          this.buildForm(qs, detail?.responses ?? []);
          
          if (isDraft) {
            this.form.enable();
          } else {
            this.form.disable();
          }
          
          this.loading.set(false);
       },
       error: () => {
          this.notify.error('Failed to load version details.');
          this.loading.set(false);
       }
    });
  }

  onVersionChange(submissionId: string | null) {
    if (!submissionId) {
       this.startNewVersion();
    } else {
       this.selectedSubmissionId.set(submissionId);
       this.loadSubmissionDetail(submissionId);
    }
  }

  startNewVersion() {
    this.isReadOnly.set(false);
    this.isCreatingNewVersion.set(true);
    this.formStatus.set('New Version');
    this.selectedSubmissionId.set(null); 
    this.form.enable();
  }

  buildForm(questions: QuestionItem[], answers: any[]): void {
    const group: Record<string, any> = {};

    questions.forEach(q => {
      const existing  = answers.find(a => a.questionId === q.id);
      const validators = [];

      if (q.isRequired && !this.isPreview && !this.isView) {
        validators.push(Validators.required);
      }
      if (q.isRequired) validators.push(Validators.required);

      if (q.fieldType === 'Number') {
        if (q.minValue !== null && q.minValue !== undefined) validators.push(Validators.min(q.minValue));
        if (q.maxValue !== null && q.maxValue !== undefined) validators.push(Validators.max(q.maxValue));
      }

      if (q.fieldType === 'TextBox' || q.fieldType === 'TextArea') {
        if (q.minLength !== null && q.minLength !== undefined) validators.push(Validators.minLength(q.minLength));
        if (q.maxLength !== null && q.maxLength !== undefined) validators.push(Validators.maxLength(q.maxLength));
      }
      if (q.fieldType === 'Checkbox') {
        const saved   = existing?.responseValues ?? [];
        const options = q.options ?? [];
        const arr = this.fb.array(
          options.map(opt => this.fb.control(saved.includes(opt.value)))
        );
        if (q.isRequired && !this.isPreview && !this.isView) {
          arr.addValidators(this.minSelectedCheckboxes(1));
        }
        group[q.id] = arr;
      }

     else if (q.fieldType === 'Dropdown' || q.fieldType === 'RadioButton') {
      let initial = existing?.responseValue ?? q.defaultValue ?? null;
      group[q.id] = [initial, validators];
    } 
    else {
      let initial = existing?.responseValue ?? q.defaultValue ?? null; 
      if (q.fieldType === 'Date' && initial) {
        initial = new Date(initial) as any;
      }
      group[q.id] = [initial, validators];
    }
    });
    this.form = this.fb.group(group);
  }

  isOptionSelected(q: QuestionItem, optValue: string): boolean {
    return this.form.get(q.id)?.value === optValue;
  }

  isCheckboxOptionSelected(q: QuestionItem, index: number): boolean {
    const arr = this.form.get(q.id) as FormArray;
    return !!arr?.at(index)?.value;
  }

  private minSelectedCheckboxes(min: number) {
    return (ctrl: AbstractControl): ValidationErrors | null => {
      if (!(ctrl instanceof FormArray)) return null;
      return ctrl.controls.filter(c => c.value).length >= min ? null : { required: true };
    };
  }

  private hasValidationErrors(): boolean {
    return this.questions().some(q => {
      const control = this.form.get(q.id);
  
      if (!control || !control.errors) {
        return false;
      }
  
      const { required, ...otherErrors } = control.errors;
  
      return Object.keys(otherErrors).length > 0;
    });
  }

  private buildPayload(notes = '') {
    const responses = this.questions().map(q => {
      const raw = this.form.getRawValue()[q.id];
      let responseValue: string | null  = null;
      let responseValues: string[] | null = null;

      if (q.fieldType === 'Checkbox') {
        responseValues = (q.options ?? [])
          .filter((_: any, i: number) => raw[i])
          .map((opt: any) => opt.value);
      } else if (q.fieldType === 'Date' && raw) {
        responseValue = new Date(raw).toISOString();
      } else {
        responseValue = raw != null ? String(raw) : null;
      }

      return { questionId: q.id, responseValue, responseValues };
    });

    return { notes, responses };
  }

  saveDraft(): void {
    if (this.isPreview || this.isReadOnly()) {
      return;
    }
  
    this.form.markAllAsTouched();
    if (this.hasValidationErrors()) {
      this.notify.error(
        'Please correct invalid values before saving the draft.'
      );
      return;
    }
  
    this.savingDraft.set(true);
  
    this.svc
      .saveDraft(
        this.assignmentId,
        this.patientId,
        this.buildPayload()
      )
      .subscribe({
        next: () => {
          this.notify.success('Draft saved successfully.');
          this.savingDraft.set(false);
          this.goBack();
        },
        error: () => this.savingDraft.set(false),
      });
  }

  submitForm(): void {
    if (this.isPreview || this.isReadOnly()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submitting.set(true);
    this.svc.submit(this.assignmentId, this.patientId, this.buildPayload()).subscribe({
      next: () => {
        this.notify.success('Questionnaire submitted successfully.');
        this.submitting.set(false);
        this.goBack();
      },
      error: () => this.submitting.set(false),
    });
  }

  goBack(): void {
    if ((this.isPreview || this.isView) && this.dialogRef) {
      this.dialogRef.close();
    } else {
      this.router.navigate(['/patient/patient-questionnaire-list']);
    }
  }
}