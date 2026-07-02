export interface InvoiceSummary {
    id: string;
    patientId: string;
    patientName: string;
    invoiceDate: string;
    status: number; 
    grandTotal: number;
    totalPaid: number;
  }

export interface InvoiceCreateDto {
  doctorId: string;
  patientId: string;
  appointmentId?: string; 
  subTotal: number;
  totalDiscount: number;
  totalTax: number;
  grandTotal: number;
  items: InvoiceItemDto[];
}

export interface InvoiceItemDto {
  billingItemId?: string | null;
  description: string;
  amount: number;
  discount: number;
  isTax: boolean;
  tax: number;
  total: number;
}

export interface BillingItemDto {
  id: string;
  itemName: string;
  description: string;
  defaultAmount: number;
  isTaxable: boolean;
}

export interface AppointmentDto {
    id: string;
    startTime: string;
    doctorNotes: string;
  }

export interface InvoiceItemDetails {
  id: string;
  billingItemId?: string | null;
  description: string;
  amount: number;
  isTax: boolean;
}

export interface InvoicePayment {
  id?: string;
  paymentDate?: string;
  paymentMode: string;
  paymentAmount: number;
}

export interface InvoiceDetails {
  id: string;
  patientId: string;
  doctorName: string;
  patientName: string;
  appointmentId?: string | null;
  startTime: string;
  invoiceDate: string; 
  doctorNotes: string;
  patientAge: string;
  patientGender: string;
  doctorSpecialization: string;
  doctorContactNumber: string;
  doctorHospital: string;
  subTotal: number;
  totalTax: number;
  grandTotal: number;
  items: InvoiceItemDetails[];
  payments: InvoicePayment[];
}