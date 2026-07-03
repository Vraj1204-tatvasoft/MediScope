import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

import { InvoiceService } from '../../services/invoice.service';
import { InvoiceDetails } from '../../models/invoice.model'; 
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-add-payment-dialog',
  templateUrl: './add-payment-dialog.component.html',
  styleUrls: ['./add-payment-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatDialogModule, MatIconModule, MatDividerModule, MatDatepickerModule, MatNativeDateModule
  ]
})
export class AddPaymentDialogComponent implements OnInit {
  paymentForm: FormGroup;
  totalPaidAmount = 0;
  paymentModes = ['Cash', 'Credit Card', 'Debit Card', 'UPI', 'Bank Transfer'];
  isLoading = false;
  protected readonly Math = Math;
  maxDate = new Date()
  invoiceData!: InvoiceDetails;

  constructor(
    private fb: FormBuilder,
    private invoiceService: InvoiceService,
    public dialogRef: MatDialogRef<AddPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { invoiceId: string; balanceDue: number }
  ) {
    this.paymentForm = this.fb.group({
      payments: this.fb.array([])
    });
  }

  ngOnInit() {
    this.loadInvoicePayments();
  }

  get payments(): FormArray {
    return this.paymentForm.get('payments') as FormArray;
  }

  loadInvoicePayments() {
    this.isLoading = true;
    this.invoiceService.getInvoiceById(this.data.invoiceId).subscribe({
      next: (invoice: any) => {
        
        this.invoiceData = invoice; 
        
        this.payments.clear();

        if (invoice && invoice.payments) {
          invoice.payments.forEach((pmt: any) => {
            const row = this.fb.group({
              paymentDate: [{ value: pmt.paymentDate || new Date(), disabled: true }, Validators.required],
              paymentMode: [{ value: pmt.paymentMode, disabled: true }, Validators.required],
              paymentAmount: [{ value: pmt.paymentAmount, disabled: true }, [Validators.required, Validators.min(1)]],
              isExisting: [true] 
            });
            this.payments.push(row);
          });
        }
        
        this.addPaymentRow();
        this.isLoading = false;
      },
      error: () => {
        this.addPaymentRow();
        this.isLoading = false;
      }
    });
  }

  addPaymentRow() {
    const currentPending = Math.max(0, this.data.balanceDue - this.totalPaidAmount);

    const paymentGroup = this.fb.group({
      paymentDate: [new Date(), Validators.required],
      paymentMode: ['Cash', Validators.required],
      paymentAmount: [currentPending > 0 ? currentPending : 0, [Validators.required, Validators.min(1)]],
      isExisting: [false] 
    });

    this.payments.push(paymentGroup);
    this.calculateTotals();

    paymentGroup.valueChanges.subscribe(() => this.calculateTotals());
  }

  removePaymentRow(index: number) {
    const isExisting = this.payments.at(index).get('isExisting')?.value;
    if (!isExisting) {
      this.payments.removeAt(index);
      this.calculateTotals();
    }
  }

  calculateTotals() {
    this.totalPaidAmount = 0;
    this.payments.getRawValue().forEach((pmt: any) => {
      const amount = Number(pmt.paymentAmount) || 0; 
      this.totalPaidAmount += amount;
    });
  }

  onSubmit() {
    if (this.paymentForm.invalid) return;

    const rawFormValues = this.paymentForm.getRawValue();
    const newPaymentsPayload = rawFormValues.payments
      .filter((pmt: any) => !pmt.isExisting)
      .map((pmt: any) => ({
        paymentDate: new Date(pmt.paymentDate).toISOString(),
        paymentMode: pmt.paymentMode,
        paymentAmount: pmt.paymentAmount
      }));

    if (newPaymentsPayload.length === 0) return;

    const payload = {
      patientId: this.invoiceData.patientId,
      appointmentId: this.invoiceData.appointmentId || null,
      subTotal: this.invoiceData.subTotal,
      totalDiscount: 0,
      totalTax: this.invoiceData.totalTax,
      grandTotal: this.invoiceData.grandTotal,
      items: this.invoiceData.items, 
      payments: newPaymentsPayload 
    };

    this.invoiceService.updateInvoice(this.data.invoiceId, payload).subscribe({
      next: () => this.dialogRef.close(true)
    });
  }
}