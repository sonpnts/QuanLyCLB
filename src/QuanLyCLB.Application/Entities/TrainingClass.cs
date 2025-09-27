namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể Lớp đào tạo mô tả thông tin lớp học và mối quan hệ với giảng viên, lịch học.
/// </summary>
public class TrainingClass : AuditableEntity
{
    // Khóa chính của lớp học
    public Guid Id { get; set; } = Guid.NewGuid();

    // Thông tin mô tả lớp học
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int MaxStudents { get; set; }

    // Quan hệ với giảng viên phụ trách
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }

    public bool IsActive { get; set; } = true;

    // Danh sách lịch học (navigation property)
    public ICollection<ClassSchedule> Schedules { get; set; } = new List<ClassSchedule>();
}
