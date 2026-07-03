import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { InvoiceService } from '../../services/invoice.service';
import { Location } from '@angular/common';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { RefundDialogComponent } from '../refund-dialog/refund-dialog.component';
import { MatDialog } from '@angular/material/dialog';
@Component({
  selector: 'app-invoice-detail',
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.css'],
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule, MatDividerModule]
})
export class InvoiceDetailComponent implements OnInit {
  invoice: any = null; 
  isLoading = true;
  
  totalPaid: number = 0;
  balanceDue: number = 0;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private invoiceService = inject(InvoiceService);
  private location = inject(Location);
  private dialog = inject(MatDialog);
  ngOnInit(): void {
    const invoiceId = this.route.snapshot.paramMap.get('id');
    if (invoiceId) {
      this.loadInvoiceDetails(invoiceId);
    }
  }

  loadInvoiceDetails(id: string) {
    this.invoiceService.getInvoiceById(id).subscribe({
      next: (res: any) => {
        this.invoice = res.data || res; 
        
        this.calculatePaymentTotals();
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load invoice', err);
        this.isLoading = false;
      }
    });
  }

  calculatePaymentTotals() {
    this.totalPaid = 0;
    
    if (this.invoice && this.invoice.payments) {
      this.invoice.payments.forEach((pmt: any) => {
        let netPayment = Number(pmt.paymentAmount) || 0;
        
        if (pmt.refunds && pmt.refunds.length > 0) {
          pmt.refunds.forEach((refund: any) => {
            netPayment -= Number(refund.refundAmount) || 0;
          });
        }
        
        this.totalPaid += netPayment;
      });
    }

    this.balanceDue = Math.max(0, (this.invoice?.grandTotal || 0) - this.totalPaid);
  }

  onRefundSpecific(pmt: any) {
    this.dialog.open(RefundDialogComponent, {
      width: '700px', 
      data: { 
        invoiceId: this.invoice.id, 
        paymentIds: [pmt.id], 
        grandTotal: this.invoice.grandTotal,
        minRefundDate: new Date(pmt.paymentDate) 
      }
    }).afterClosed().subscribe(res => {
      if (res) {
        this.isLoading = true;
        this.loadInvoiceDetails(this.invoice.id);
      }
    });
  }

  goBack() {
    this.location.back();
  }

  onPrint() {
    const element = document.getElementById('invoice-content');
    
    if (element) {
      html2canvas(element, { scale: 2 }).then(canvas => {
        const imgData = canvas.toDataURL('image/png');
        const pdf = new jsPDF({
          orientation: 'p',
          unit: 'mm',
          format: 'a4',
          compress: true 
        });
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = (canvas.height * pdfWidth) / canvas.width;
        pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
        const fileName = `Invoice_${this.invoice?.patientName?.replace(/\s+/g, '_') || 'Document'}.pdf`;
        pdf.save(fileName);
      });
    }
  }
}