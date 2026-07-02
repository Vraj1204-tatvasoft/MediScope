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
import { AppointmentService } from '../../services/appointment.service'; 
import { BillingItemDto, AppointmentDto } from '../../models/invoice.model'; 

@Component({
  selector: 'app-invoice-dialog',
  templateUrl: './add-invoice-dialog.component.html',
  styleUrls: ['./add-invoice-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatButtonModule, MatDialogModule, MatIconModule, MatDividerModule
  ]
})
export class InvoiceDialogComponent implements OnInit {
  invoiceForm: FormGroup;
  patients: any[] = []; 
  billingItems: BillingItemDto[] = [];
  patientAppointments: AppointmentDto[] = [];
  
  subTotal = 0;
  totalTax = 0;
  grandTotal = 0;

  isEditMode = false;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private invoiceService: InvoiceService,
    private appointmentService: AppointmentService,
    public dialogRef: MatDialogRef<InvoiceDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { doctorId: string; invoiceId?: string }
  ) {
    this.invoiceForm = this.fb.group({
      patientId: ['', Validators.required],
      appointmentId: [''], 
      items: this.fb.array([]) 
    });

    this.isEditMode = !!this.data.invoiceId;
  }

  ngOnInit() {
    this.fetchMasterData();
    this.setupSubscriptions();
  }

  fetchMasterData() {
    this.appointmentService.getDoctorPatients().subscribe({
      next: (res: any) => { this.patients = res.data || []; }
    }); 

    this.invoiceService.getBillingItems().subscribe({
      next: (items: any) => {
        this.billingItems = Array.isArray(items) ? items : (items.data || []);
        
        if (this.isEditMode && this.data.invoiceId) {
          this.loadExistingInvoice();
        } else {
          this.addItem();
        }
      }
    });
  }

  loadExistingInvoice() {
    this.isLoading = true;
    this.invoiceService.getInvoiceById(this.data.invoiceId!).subscribe({
      next: (invoice: any) => {
        this.invoiceForm.patchValue({
          patientId: invoice.patientId,
          appointmentId: invoice.appointmentId || null
        });

        if (invoice.patientId) {
          this.appointmentService.getAppointmentsByPatient(invoice.patientId).subscribe({
            next: (appointments) => this.patientAppointments = appointments
          });
        }

        this.invoiceForm.get('patientId')?.disable();
        this.invoiceForm.get('appointmentId')?.disable();

        this.items.clear();
        invoice.items.forEach((item: any) => {
          this.items.push(this.fb.group({
            billingItemId: [item.billingItemId || null],
            description: [item.description, Validators.required],
            amount: [item.amount, [Validators.required, Validators.min(0)]],
            taxRate: [item.taxRate || (item.isTax ? 10 : 0), [Validators.required, Validators.min(0)]]
          }));
        });

        this.isLoading = false;
        this.calculateTotals();
      },
      error: () => this.isLoading = false
    });
  }

  setupSubscriptions() {
    this.invoiceForm.get('patientId')?.valueChanges.subscribe(patientId => {
      if (this.isEditMode) return; 
      
      this.invoiceForm.get('appointmentId')?.reset(); 
      if (patientId) {
        this.appointmentService.getAppointmentsByPatient(patientId).subscribe({
          next: (appointments) => this.patientAppointments = appointments
        });
      } else {
        this.patientAppointments = [];
      }
    });

    this.invoiceForm.valueChanges.subscribe(() => this.calculateTotals());
  }

  get items(): FormArray {
    return this.invoiceForm.get('items') as FormArray;
  }

  addItem() {
    const itemGroup = this.fb.group({
      billingItemId: [null], 
      description: ['', Validators.required],
      amount: [0, [Validators.required, Validators.min(0)]],
      taxRate: [0, [Validators.required, Validators.min(0)]]
    });
    this.items.push(itemGroup);
  }

  removeItem(index: number) {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  onBillingItemChange(billingItemId: string, index: number) {
    const selectedItem = this.billingItems.find(b => b.id === billingItemId);
    if (selectedItem) {
      const formGroup = this.items.at(index);
      formGroup.patchValue({
        description: selectedItem.itemName,   
        amount: selectedItem.defaultAmount, 
        taxRate: selectedItem.isTaxable ? 10 : 0     
      });
    }
  }

  isBillingItemDisabled(billingItemId: string, currentIndex: number): boolean {
    if (!billingItemId) return false;
    return this.items.controls.some((control, index) => 
      index !== currentIndex && control.get('billingItemId')?.value === billingItemId
    );
  }

  calculateTotals() {
    this.subTotal = 0;
    this.totalTax = 0;

    this.items.controls.forEach(control => {
      const amount = Number(control.get('amount')?.value) || 0;
      const taxRate = Number(control.get('taxRate')?.value) || 0;
      this.subTotal += amount;
      this.totalTax += (amount * (taxRate / 100));
    });

    this.grandTotal = this.subTotal + this.totalTax;
  }

  onSubmit() {
    if (this.invoiceForm.invalid || this.items.length === 0) return;

    const formVal = this.invoiceForm.getRawValue();

    const invoiceItems = formVal.items.map((item: any) => {
      const rate = Number(item.taxRate) || 0;
      const itemTax = item.amount * (rate / 100);
      return {
        billingItemId: item.billingItemId || null, 
        description: item.description,
        amount: item.amount,
        discount: 0,
        isTax: rate > 0,
        taxRate: rate,
        tax: itemTax,
        total: item.amount + itemTax
      };
    });

    const payload = {
      doctorId: this.data.doctorId,
      patientId: formVal.patientId,
      appointmentId: formVal.appointmentId || null,
      subTotal: this.subTotal,
      totalDiscount: 0,
      totalTax: this.totalTax,
      grandTotal: this.grandTotal,
      items: invoiceItems
    };

    if (this.isEditMode && this.data.invoiceId) {
      this.invoiceService.updateInvoice(this.data.invoiceId, payload).subscribe({
        next: () => this.dialogRef.close(true)
      });
    } else {
      this.invoiceService.createInvoice(payload).subscribe({
        next: () => this.dialogRef.close(true)
      });
    }
  }
}