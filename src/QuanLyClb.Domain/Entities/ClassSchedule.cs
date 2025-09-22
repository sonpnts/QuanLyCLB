namespace QuanLyClb.Domain.Entities;

public class ClassSchedule : BaseEntity
{
    public Guid ClassId { get; set; }
    public TrainingClass Class { get; set; } = default!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Location { get; set; }
}
