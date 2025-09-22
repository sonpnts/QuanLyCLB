using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class AttendanceRecord : BaseEntity
{
    public Guid AttendanceSessionId { get; set; }
    public AttendanceSession Session { get; set; } = default!;
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
}
