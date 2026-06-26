using System;
using System.Collections.Generic;

namespace MediScope.Common.Models.DTOs.Response
{
    public class InvoiceSummaryDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int Status { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalPaid { get; set; }
    }
}