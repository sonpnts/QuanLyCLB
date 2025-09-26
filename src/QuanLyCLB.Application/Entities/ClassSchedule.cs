using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể Lịch học cho biết một buổi học cụ thể của lớp đào tạo.
/// </summary>
public class ClassSchedule
{
    // Khóa chính và liên kết về lớp đào tạo
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainingClassId { get; set; }
    public TrainingClass? TrainingClass { get; set; }

    // Thông tin thời gian diễn ra buổi học
    public DateOnly StudyDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }

    // Thông tin địa điểm và bán kính được phép điểm danh
    public string LocationName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AllowedRadiusMeters { get; set; }

    public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public TimeOnly CreatedTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);

    public bool IsActive { get; set; } = true;

    // Danh sách bản ghi điểm danh của buổi học
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
