using System;
using System.Collections.Generic;

namespace MediScope.Common.Models.DTOs.Response
{
    public class InvoiceDetailsDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public Guid? AppointmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string DoctorNotes { get; set; }
        public string PatientAge { get; set; }
        public string PatientGender { get; set; }
        public string DoctorSpecialization { get; set; }
        public string DoctorContactNumber { get; set; }
        public string DoctorHospital { get; set; }
        public decimal SubTotal
        { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
        public List<InvoicePaymentResponseDto> Payments { get; set; } = new();
    }

    public class InvoiceItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid? BillingItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsTax { get; set; }
    }
    public class InvoicePaymentResponseDto
    {
        public Guid Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
    }
}