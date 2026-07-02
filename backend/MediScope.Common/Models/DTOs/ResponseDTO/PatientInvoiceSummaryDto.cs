using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediScope.Common.Models.DTOs.Response
{
    public class PatientInvoiceSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int Status { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalPaid { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
}