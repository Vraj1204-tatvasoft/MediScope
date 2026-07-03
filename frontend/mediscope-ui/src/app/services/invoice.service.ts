import { Injectable } from '@angular/core';
import { BaseHttpService } from './base-http.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BillingItemDto, InvoiceCreateDto, InvoiceDetails, InvoiceSummary, RefundRequestDto } from '../models/invoice.model'; 

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly endpoint = 'invoices'; 

  constructor(private baseHttp: BaseHttpService) {}

  createInvoice(data: InvoiceCreateDto): Observable<string> {
    return this.baseHttp.post<string>(this.endpoint, data, { 
      showSuccess: true, 
      showError: true 
    }).pipe(
      map(response => response.data)
    );
  }

  deleteInvoice(invoiceId: string): Observable<void> {
    return this.baseHttp.delete<void>(`${this.endpoint}/${invoiceId}`, {
      showSuccess: true, 
      showError: true
    }).pipe( map(() => void 0) );
  }

  getBillingItems(): Observable<BillingItemDto[]> {
        return this.baseHttp.get<BillingItemDto[]>(`${this.endpoint}/billing-items`).pipe(
            map(res => res.data ? res.data : (Array.isArray(res) ? res : []))
        );
  }

  getInvoiceById(invoiceId: string): Observable<InvoiceDetails> {
    return this.baseHttp.get<any>(`${this.endpoint}/${invoiceId}`).pipe(
      map(res => (res.data ? res.data : res) as InvoiceDetails)
    );
  }

  updateInvoice(invoiceId: string, data: any): Observable<void> {
    return this.baseHttp.put<void>(`${this.endpoint}/${invoiceId}`, data, {
      showSuccess: true,
      showError: true
    }).pipe(
      map(() => void 0)
    );
  }

  getMyInvoices(): Observable<InvoiceSummary[]> {
    return this.baseHttp.get<InvoiceSummary[]>(this.endpoint).pipe(
        map((response: any) => response.data || response)
    );
  }

  issueRefund(invoiceId: string, data: RefundRequestDto): Observable<void> {
    return this.baseHttp.post<void>(`${this.endpoint}/${invoiceId}/refund`, data, {
      showSuccess: true,
      showError: true
    }).pipe(map(() => void 0));
  }
}