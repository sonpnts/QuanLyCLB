using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.Entities;

public class AttendanceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
    public DateTime CheckedInAt { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? TicketId { get; set; }
    public AttendanceTicket? Ticket { get; set; }
}
