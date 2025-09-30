namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Quy định mức lương áp dụng theo vai trò và trình độ kỹ năng.
/// </summary>
public class PayrollRule : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tên vai trò áp dụng (ví dụ: Coach, Assistant).
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Trình độ chuyên môn tương ứng (ví dụ: Đai nâu, Đai đen 1 đẳng).
    /// </summary>
    public string SkillLevel { get; set; } = string.Empty;

    /// <summary>
    /// Mức lương theo giờ áp dụng cho quy định này.
    /// </summary>
    public decimal HourlyRate { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
