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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InvoiceService } from '../../services/invoice.service';
import { InvoiceDetails } from '../../models/invoice.model';
import { environment } from '../../../environments/environments';

declare var Razorpay: any;

@Component({
  selector: 'app-add-payment-dialog',
  templateUrl: './add-payment-dialog.component.html',
  styleUrls: ['./add-payment-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatDialogModule, MatIconModule,
    MatDividerModule, MatDatepickerModule, MatNativeDateModule,
    MatCheckboxModule, MatProgressSpinnerModule
  ]
})
export class AddPaymentDialogComponent implements OnInit {
  paymentForm: FormGroup;
  totalPaidAmount     = 0;
  paymentModes        = ['Cash', 'Credit Card', 'Debit Card'];
  isLoading           = false;
  isProcessingPayment = false;
  protected readonly Math = Math;
  maxDate     = new Date();
  invoiceData!: InvoiceDetails;

  private readonly gatewayModes = ['Credit Card', 'Debit Card'];

  constructor(
    private fb: FormBuilder,
    private invoiceService: InvoiceService,
    public dialogRef: MatDialogRef<AddPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { invoiceId: string; balanceDue: number }
  ) {
    this.paymentForm = this.fb.group({ payments: this.fb.array([]) });
  }

  ngOnInit() {
    this.loadInvoicePayments();
  }

  get payments(): FormArray {
    return this.paymentForm.get('payments') as FormArray;
  }

  // ── Load existing payment history ────────────────────────────
  loadInvoicePayments() {
    this.isLoading = true;
    this.invoiceService.getInvoiceById(this.data.invoiceId).subscribe({
      next: (invoice: any) => {
        this.invoiceData = invoice;
        this.payments.clear();

        if (invoice?.payments) {
          invoice.payments.forEach((pmt: any) => {
            const isRefunded = pmt.refunds && pmt.refunds.length > 0;
            const row = this.fb.group({
              paymentDate:   [{ value: pmt.paymentDate || new Date(), disabled: true }, Validators.required],
              paymentMode:   [{ value: pmt.paymentMode, disabled: true }, Validators.required],
              paymentAmount: [{ value: pmt.paymentAmount, disabled: true }, [Validators.required, Validators.min(1)]],
              saveCard:      [{ value: false, disabled: true }],
              isExisting:    [true],
              isRefunded:    [isRefunded]
            });
            this.payments.push(row);
          });
        }

        this.calculateTotals();
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
    const pending = Math.max(0, this.data.balanceDue - this.totalPaidAmount);
    const group   = this.fb.group({
      paymentDate:   [new Date(), Validators.required],
      paymentMode:   ['Cash', Validators.required],
      paymentAmount: [pending > 0 ? pending : 0, [Validators.required, Validators.min(1)]],
      saveCard:      [false],
      isExisting:    [false],
      isRefunded:    [false]
    });
    this.payments.push(group);
    this.calculateTotals();
    group.valueChanges.subscribe(() => this.calculateTotals());
  }

  removePaymentRow(index: number) {
    if (!this.payments.at(index).get('isExisting')?.value) {
      this.payments.removeAt(index);
      this.calculateTotals();
    }
  }

  calculateTotals() {
    this.totalPaidAmount = this.payments.getRawValue()
      .filter((pmt: any) => pmt.isExisting ? !pmt.isRefunded : true)
      .reduce((sum: number, pmt: any) => sum + (Number(pmt.paymentAmount) || 0), 0);
  }

  requiresGateway(paymentMode: string): boolean {
    return this.gatewayModes.includes(paymentMode);
  }

  isCardMode(paymentMode: string): boolean {
    return paymentMode === 'Credit Card' || paymentMode === 'Debit Card';
  }

  onSubmit() {
    if (this.paymentForm.invalid || this.isProcessingPayment) return;
    const rawValues   = this.paymentForm.getRawValue();
    const newPayments = rawValues.payments.filter((p: any) => !p.isExisting);
    if (newPayments.length === 0) return;
    this.processNextPayment(newPayments, 0);
  }

  // ── Sequential payment processor ────────────────────────────
  private processNextPayment(payments: any[], index: number) {
    if (index >= payments.length) {
      this.dialogRef.close(true);
      return;
    }

    const pmt = payments[index];

    if (this.requiresGateway(pmt.paymentMode)) {
      // Razorpay handles saved card selection internally via OTP unlock
      // No custom card selection dialog needed
      this.openRazorpay(pmt, () => this.processNextPayment(payments, index + 1));
    } else {
      // Cash — call backend directly, no popup
      this.isProcessingPayment = true;
      this.invoiceService.createPaymentOrder(
        this.data.invoiceId, pmt.paymentAmount, pmt.paymentMode
      ).subscribe({
        next: () => {
          this.isProcessingPayment = false;
          this.processNextPayment(payments, index + 1);
        },
        error: (err) => {
          console.error('Cash payment failed', err);
          this.isProcessingPayment = false;
        }
      });
    }
  }

  // ── Open Razorpay checkout ───────────────────────────────────
  private openRazorpay(pmt: any, onSuccess: () => void) {
    this.isProcessingPayment = true;

    this.invoiceService.createPaymentOrder(
      this.data.invoiceId, pmt.paymentAmount, pmt.paymentMode
    ).subscribe({
      next: (response: any) => {
        const order = response.data;

        const options: any = {
          key:         environment.razorpayKey,
          amount:      Math.round(order.amount * 100),
          currency:    'USD',           // must be INR for saved cards + tokenization
          order_id:    order.orderId,
          name:        'MediScope',
          description: 'Invoice Payment',
          customer_id: order.razorpayCustomerId ?? undefined,
          remember_customer: true,
          // save: 1 — shows "Save this card as per RBI guidelines" checkbox
          // Razorpay also uses this to unlock saved cards list via OTP
          //save: 1,

          prefill: {
            contact: order.patientContact ?? '',
            email:   order.patientEmail   ?? ''
          },

          handler: (razorpayResponse: any) => {
            console.log('Razorpay response:', razorpayResponse);

            const verifyPayload = {
              invoiceId:         this.data.invoiceId,
              amount:            pmt.paymentAmount,
              paymentMode:       pmt.paymentMode,
              razorpayOrderId:   razorpayResponse.razorpay_order_id,
              razorpayPaymentId: razorpayResponse.razorpay_payment_id,
              razorpaySignature: razorpayResponse.razorpay_signature,
              saveCard:          pmt.saveCard ?? false,
              paymentDate:       new Date().toISOString()
            };

            console.log('Verify payload:', verifyPayload);

            this.invoiceService.verifyPayment(verifyPayload).subscribe({
              next: () => {
                this.isProcessingPayment = false;
                onSuccess();
              },
              error: (err) => {
                console.error('Verification failed:', err);
                this.isProcessingPayment = false;
              }
            });
          },

          modal: {
            ondismiss: () => {
              this.isProcessingPayment = false;
            }
          }
        };

        const rzp = new Razorpay(options);
        rzp.open();
      },
      error: (err) => {
        console.error('Order creation failed', err);
        this.isProcessingPayment = false;
      }
    });
  }
}