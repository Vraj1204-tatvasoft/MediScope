using System;
using System.Collections.Generic;
using MediScope.Common.Models.Enums;

namespace MediScope.Common.Models.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }

        public decimal TotalPaid { get; set; }
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}