namespace QuanLyCLB.API.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public StudentDto Student { get; set; } = null!;
        public ClassDto? Class { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        public int StudentId { get; set; }
        public int? ClassId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentType { get; set; } // PaymentType as int
        public DateTime DueDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalPayments { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public List<PaymentDto> Payments { get; set; } = new();
    }
}