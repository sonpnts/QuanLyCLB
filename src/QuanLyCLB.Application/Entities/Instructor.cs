namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể Giảng viên nằm ở tầng nghiệp vụ (Application), chứa thông tin quản lý huấn luyện viên.
/// </summary>
public class Instructor
{
    // Khóa chính dạng GUID để đảm bảo dữ liệu đồng nhất giữa các tầng
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public UserAccount User { get; set; } = null!;

    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property thể hiện quan hệ với các thực thể khác
    public ICollection<TrainingClass> Classes { get; set; } = new List<TrainingClass>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<PayrollPeriod> Payrolls { get; set; } = new List<PayrollPeriod>();
}
