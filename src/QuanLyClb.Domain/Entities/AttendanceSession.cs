namespace QuanLyClb.Domain.Entities;

public class AttendanceSession : BaseEntity
{
    public Guid ClassId { get; set; }
    public TrainingClass Class { get; set; } = default!;
    public DateTime SessionDate { get; set; }
    public Guid? CoachId { get; set; }
    public User? Coach { get; set; }
    public Guid? MarkedById { get; set; }
    public User? MarkedBy { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }

    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}
