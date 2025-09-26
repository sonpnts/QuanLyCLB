namespace QuanLyCLB.Application.Entities;

public class PayrollPeriod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PayrollDetail> Details { get; set; } = new List<PayrollDetail>();
}

public class PayrollDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PayrollPeriodId { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }
    public Guid AttendanceRecordId { get; set; }
    public AttendanceRecord? AttendanceRecord { get; set; }
    public decimal Hours { get; set; }
    public decimal Amount { get; set; }
}
