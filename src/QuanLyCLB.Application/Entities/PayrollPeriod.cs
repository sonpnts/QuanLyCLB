namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể bảng lương theo tháng dành cho từng giảng viên.
/// </summary>
public class PayrollPeriod : AuditableEntity
{
    // Khóa chính và liên kết với giảng viên
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CoachId { get; set; }
    public UserAccount? Coach { get; set; }

    // Thông tin kỳ lương
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Chi tiết lương theo từng buổi dạy
    public ICollection<PayrollDetail> Details { get; set; } = new List<PayrollDetail>();
}

/// <summary>
/// Chi tiết tiền công tương ứng với từng bản ghi điểm danh.
/// </summary>
public class PayrollDetail : AuditableEntity
{
    // Khóa chính và liên kết với kỳ lương, bản ghi điểm danh
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PayrollPeriodId { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }
    public Guid AttendanceRecordId { get; set; }
    public AttendanceRecord? AttendanceRecord { get; set; }

    // Giá trị giờ dạy và tiền công tương ứng
    public decimal Hours { get; set; }
    public decimal Amount { get; set; }

    public bool IsActive { get; set; } = true;
}
