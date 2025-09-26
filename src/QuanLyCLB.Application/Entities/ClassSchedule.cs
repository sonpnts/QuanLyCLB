using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.Entities;

public class ClassSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainingClassId { get; set; }
    public TrainingClass? TrainingClass { get; set; }
    public DateOnly StudyDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AllowedRadiusMeters { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
