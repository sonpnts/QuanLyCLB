namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể Phiếu hỗ trợ điểm danh giúp quản lý yêu cầu điều chỉnh.
/// </summary>
public class AttendanceTicket
{
    // Thông tin khóa chính và liên kết đến buổi học/giảng viên
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }

    // Chi tiết nội dung phiếu
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Quan hệ 1-1 với bản ghi điểm danh sau khi xử lý
    public AttendanceRecord? AttendanceRecord { get; set; }
}
