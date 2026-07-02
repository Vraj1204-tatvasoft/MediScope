import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';

import { InvoiceService } from '../../services/invoice.service';
import { AddPaymentDialogComponent } from '../../doctor/add-payment-dialog/add-payment-dialog.component';

@Component({
  selector: 'app-patient-invoices',
  templateUrl: './patient-invoice.component.html',
  styleUrls: ['./patient-invoice.component.css'],
  standalone: true,
  imports: [
    CommonModule, MatIconModule, MatButtonModule, MatTableModule, 
    MatDialogModule, MatMenuModule
  ]
})
export class PatientInvoicesComponent implements OnInit {
  displayedColumns: string[] = ['invoiceDate', 'doctorId', 'grandTotal', 'totalPaid', 'status', 'actions'];
  invoices: any[] = [];
  
  constructor(
    private invoiceService: InvoiceService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.invoiceService.getMyInvoices().subscribe({
      next: (data) => {
        this.invoices = data || [];
      },
      error: (err) => console.error('Failed to load patient invoices:', err)
    });
  }

  onView(invoice: any): void {
    this.router.navigate(['/patient/invoice-detail', invoice.id]); 
  }

  onAddPayment(element: any): void {
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
        this.loadInvoices(); // Refresh table when payment is added
      }
    });
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