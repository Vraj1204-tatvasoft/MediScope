import {
  Component, OnInit, inject, signal, Inject, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize, switchMap, of } from 'rxjs'; // <-- Added switchMap and of

import { BroadcastService } from '../../services/admin-broadcast.service';
import { BroadcastDetail, CHANNEL_INT, AUDIENCE_INT } from '../../models/admin-broadcast.model';

@Component({
  selector: 'app-broadcast-form-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatIconModule, MatProgressSpinnerModule,
  ],
  templateUrl: './broadcast-form-modal.component.html',
  styleUrls: ['./broadcast-form-modal.component.css']
})
export class BroadcastFormModalComponent implements OnInit {
  private readonly fb        = inject(FormBuilder);
  private readonly svc       = inject(BroadcastService);
  private readonly dialogRef = inject(MatDialogRef<BroadcastFormModalComponent>);
  readonly data: BroadcastDetail | null = inject(MAT_DIALOG_DATA, { optional: true });

  form!: FormGroup;
  saving        = signal(false);
  sendNow       = signal(false);
  audienceCount = signal<number | null>(null);
  countLoading  = signal(false);

  get isEdit(): boolean { return !!this.data; }

  ngOnInit(): void {
    const channelInt  = this.data ? CHANNEL_INT[this.data.channel]   : 0;
    const audienceInt = this.data ? AUDIENCE_INT[this.data.audience] : 3;

    this.form = this.fb.group({
      name:      [this.data?.name    ?? '', Validators.required],
      channel:   [channelInt,               Validators.required],
      subject:   [this.data?.subject ?? ''],
      message:   [this.data?.message ?? '', Validators.required],
      audience:  [audienceInt,              Validators.required],
      batchSize: [this.data?.batchSize ?? 100],
    });

    this.updateSubjectValidator();
    this.loadAudienceCount();
  }

  onChannelChange(): void  { this.updateSubjectValidator(); }
  onAudienceChange(): void { this.loadAudienceCount(); }

  private updateSubjectValidator(): void {
    const subjectCtrl = this.form.get('subject')!;
    if (this.form.get('channel')?.value === 0) {
      subjectCtrl.setValidators([Validators.required]);
    } else {
      subjectCtrl.clearValidators();
      subjectCtrl.setValue('');
    }
    subjectCtrl.updateValueAndValidity();
  }

  private loadAudienceCount(): void {
    const audience = this.form.get('audience')?.value as number;
    this.countLoading.set(true);
    this.svc.getAudienceCount(audience)
      .pipe(finalize(() => this.countLoading.set(false)))
      .subscribe({
        next: res => this.audienceCount.set(res.data?.totalRecipients ?? null),
        error: ()  => this.audienceCount.set(null),
      });
  }

  touched(field: string): boolean {
    const c = this.form.get(field);
    return !!(c?.invalid && (c.dirty || c.touched));
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const val = this.form.value;
    this.saving.set(true);

    if (this.isEdit) {
      this.svc.updateBroadcast(this.data!.id, {
        name:     val.name,
        channel:  val.channel,
        subject:  val.subject || null,
        message:  val.message,
        audience: val.audience,
      }).pipe(finalize(() => this.saving.set(false)))
        .subscribe({ next: () => this.dialogRef.close(true) });
        
    } else {
      this.svc.createBroadcast({
        name:      val.name,
        channel:   val.channel,
        subject:   val.subject || null,
        message:   val.message,
        audience:  val.audience,
        sendNow:   false, 
        batchSize: val.batchSize,
      }).pipe(
        switchMap((res: any) => {
          if (this.sendNow()) {
            const newBroadcastId = res.data.id;
            return this.svc.sendBroadcast(newBroadcastId);
          }
          return of(res);
        }),
        finalize(() => this.saving.set(false))
      )
      .subscribe({ 
        next: () => this.dialogRef.close(true),
        error: (err) => {
          console.error('Error processing broadcast:', err);
        }
      });
    }
  }

  cancel(): void { this.dialogRef.close(false); }
}