using System.ComponentModel.DataAnnotations;

namespace QuanLyCLB.API.Models
{
    public class Payment
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int StudentId { get; set; }
        public int? ClassId { get; set; } // Optional, for class-specific payments
        
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        
        [StringLength(100)]
        public string? PaymentMethod { get; set; } // Cash, Transfer, etc.
        
        [StringLength(255)]
        public string? TransactionId { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Student Student { get; set; } = null!;
        public Class? Class { get; set; }
    }

    public enum PaymentType
    {
        MonthlyFee = 1,
        RegistrationFee = 2,
        Equipment = 3,
        Event = 4,
        Other = 5
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Overdue = 3,
        Cancelled = 4
    }
}