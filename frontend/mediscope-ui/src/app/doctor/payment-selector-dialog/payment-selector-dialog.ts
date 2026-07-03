import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { InvoiceService } from '../../services/invoice.service';

@Component({
  selector: 'app-payment-selector',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatListModule, MatButtonModule, MatCheckboxModule],
  templateUrl: './payment-selector-dialog.html'
})
export class PaymentSelectorDialog implements OnInit {
  availablePayments: any[] = [];
  constructor(
    private invoiceService: InvoiceService,
    public dialogRef: MatDialogRef<PaymentSelectorDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { invoiceId: string }
  ) {}

  ngOnInit() {
    this.invoiceService.getInvoiceById(this.data.invoiceId).subscribe(res => {
      this.availablePayments = res.payments.filter((p: any) => !p.refunds || p.refunds.length === 0);
    });
  }

  getSelectionData(list: any): { ids: string[], latestDate: Date } {
    const selectedOptions = list.selectedOptions.selected;
    const ids = selectedOptions.map((option: any) => option.value);
    const selectedPayments = this.availablePayments.filter(p => ids.includes(p.id));
    const dates = selectedPayments.map(p => new Date(p.paymentDate).getTime());
    const latestDate = new Date(Math.min(...dates));

    return { ids: ids, latestDate: latestDate };
  }
  toggleSelectAll(list: any, event: any) {
    if (event.checked) {
      list.selectAll();
    } else {
      list.deselectAll();
    }
  }

  isAllSelected(list: any): boolean {
    const selectedCount = list?.selectedOptions?.selected?.length || 0;
    return selectedCount === this.availablePayments.length && this.availablePayments.length > 0;
  }

  isSomeSelected(list: any): boolean {
    const selectedCount = list?.selectedOptions?.selected?.length || 0;
    return selectedCount > 0 && selectedCount < this.availablePayments.length;
  }
}