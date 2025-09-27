using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể ghi nhận thông tin điểm danh từng buổi học của giảng viên.
/// </summary>
public class AttendanceRecord : AuditableEntity
{
    // Thông tin khóa chính và quan hệ với lịch học, giảng viên
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }

    // Thời gian và tọa độ điểm danh
    public DateTime CheckedInAt { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    // Trạng thái và ghi chú xử lý điểm danh
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Liên kết với phiếu hỗ trợ/giải trình điểm danh nếu có
    public Guid? TicketId { get; set; }
    public AttendanceTicket? Ticket { get; set; }
}
