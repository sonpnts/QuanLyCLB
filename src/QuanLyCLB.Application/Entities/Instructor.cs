namespace QuanLyCLB.Application.Entities;

public class Instructor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string? GoogleSubject { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TrainingClass> Classes { get; set; } = new List<TrainingClass>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<PayrollPeriod> Payrolls { get; set; } = new List<PayrollPeriod>();
}
