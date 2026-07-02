using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediScope.Common.Models.DTOs.Response
{
    public class DoctorInvoiceSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int Status { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalPaid { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
    }
}