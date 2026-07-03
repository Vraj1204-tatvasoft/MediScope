import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { InvoiceService } from '../../services/invoice.service';
import { RefundRequestDto } from '../../models/invoice.model';

@Component({
  selector: 'app-refund-dialog',
  templateUrl: './refund-dialog.component.html',
  styleUrls: ['../add-payment-dialog/add-payment-dialog.component.css'], // Reuse styling
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatDialogModule, MatDatepickerModule, MatNativeDateModule
  ]
})
export class RefundDialogComponent implements OnInit {
  refundForm: FormGroup;
  refundModes = ['Cash', 'Credit Card', 'Bank Transfer', 'UPI'];
  maxDate = new Date();
  constructor(
    private fb: FormBuilder,
    private invoiceService: InvoiceService,
    public dialogRef: MatDialogRef<RefundDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { 
        invoiceId: string, 
        paymentIds?: string, 
        grandTotal: number,
        minRefundDate?: Date
    }
  ) {
    this.refundForm = this.fb.group({
      refundMode: ['Cash', Validators.required],
      refundDate: [new Date(), Validators.required],
      reason: ['']
    });
  }

  ngOnInit() {}

  onSubmit() {
    if (this.refundForm.invalid) return;

    const val = this.refundForm.value;
    const payload: RefundRequestDto = {
      paymentIds: this.data.paymentIds && this.data.paymentIds.length > 0 ? this.data.paymentIds : null, 
      invoiceId: this.data.invoiceId,
      refundMode: val.refundMode,
      reason: val.reason,
      refundDate: new Date(val.refundDate).toISOString(),
      grandTotal: this.data.grandTotal
    };

    this.invoiceService.issueRefund(this.data.invoiceId, payload).subscribe({
      next: () => this.dialogRef.close(true)
    });
}
}