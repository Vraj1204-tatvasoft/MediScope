import { Component, OnInit, inject } from '@angular/core';
import { InvoiceDetails, InvoiceSummary } from '../../models/invoice.model';
import { InvoiceService } from '../../services/invoice.service';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { FullCalendarModule } from '@fullcalendar/angular';
import { switchMap } from 'rxjs/operators';
import { InvoiceDialogComponent } from '../add-invoice/add-invoice-dialog.component';
import { DoctorService } from '../../services/doctor.service';
//import { EditInvoiceComponent } from '../edit-invoice.component/edit-invoice.component';
import { Router } from '@angular/router';
import { AddPaymentDialogComponent } from '../add-payment-dialog/add-payment-dialog.component';

@Component({
  selector: 'app-doctor-invoices',
  templateUrl: './doctor-invoices.component.html',
  styleUrls: ['./doctor-invoices.component.css'],
  imports: [
    CommonModule,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatDialogModule,
    MatMenuModule,
    MatDividerModule,
    FullCalendarModule,
    MatMenuModule
  ]
})
export class DoctorInvoicesComponent implements OnInit {
  displayedColumns: string[] = ['invoiceDate', 'patientId', 'grandTotal', 'totalPaid', 'status', 'actions'];
  invoices: InvoiceSummary[] = [];
  
  currentUserId: string = '';
  constructor(
  private invoiceService: InvoiceService,
  private doctorService: DoctorService,
  private dialog: MatDialog,
  private router: Router
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.doctorService.getMyProfile().pipe(
      switchMap((profile) => {
        this.currentUserId = profile.doctorId; 
        return this.invoiceService.getMyInvoices();})
    ).subscribe({
      next: (data) => {
        this.invoices = data || [];
      },
      error: (err) => console.error('Failed to load doctor profile or invoices:', err)
    });
  }

  onAddInvoice(): void {
    const dialogRef = this.dialog.open(InvoiceDialogComponent, {
      width: '700px',
      disableClose: true,
      data: { doctorId: this.currentUserId }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.loadInvoices(); 
      }
    });
  }

  onEdit(invoice: any): void {
    const dialogRef = this.dialog.open(InvoiceDialogComponent, {
      width: '700px',
      disableClose: true,
      data: { 
        invoiceId: invoice.id, 
        doctorId: this.currentUserId
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.loadInvoices(); 
      }
    });
  }

  onEditPayment(element: any): void {
    const dialogRef = this.dialog.open(AddPaymentDialogComponent, {
      width: '700px',
      disableClose: true,
      data: { 
        invoiceId: element.id,           
        balanceDue: element.grandTotal - element.totalPaid     
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.loadInvoices(); 
      }
    });
  }

  onView(invoice: InvoiceSummary | InvoiceDetails): void {
    this.router.navigate(['/doctor/invoice-detail', invoice.id]); 
  }

  onDelete(invoiceId: string): void {
    if (confirm('Are you sure you want to delete this invoice?')) {
      this.invoiceService.deleteInvoice(invoiceId).subscribe({
        next: () => {
          this.invoices = this.invoices.filter(i => i.id !== invoiceId);
        },
        error: (err: { error: { message: any; }; }) => {
          alert(err.error?.message || err.error || 'Cannot delete invoice.');
        }
      });
    }
  }

  getStatusText(status: number): string {
    switch(status) {
      case 0: return 'Unpaid';
      case 1: return 'Partial';
      case 2: return 'Paid';
      case 3: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: number): string {
    switch(status) {
      case 0: return 'status-unpaid';
      case 1: return 'status-partial';
      case 2: return 'status-paid';
      case 3: return 'status-cancelled';
      default: return 'status-default';
    }
  }
}