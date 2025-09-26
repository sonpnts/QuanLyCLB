namespace QuanLyCLB.Application.Entities;

public class AttendanceTicket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public AttendanceRecord? AttendanceRecord { get; set; }
}
