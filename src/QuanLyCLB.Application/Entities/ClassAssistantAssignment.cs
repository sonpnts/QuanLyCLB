namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Phân công trợ giảng cho từng lớp hoặc lịch học cụ thể.
/// </summary>
public class ClassAssistantAssignment : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TrainingClassId { get; set; }
    public TrainingClass? TrainingClass { get; set; }

    public Guid? ClassScheduleId { get; set; }
    public ClassSchedule? ClassSchedule { get; set; }

    public Guid AssistantId { get; set; }
    public UserAccount? Assistant { get; set; }

    /// <summary>
    /// Ghi nhận vai trò đảm nhiệm trong buổi học (mặc định là Assistant).
    /// </summary>
    public string RoleName { get; set; } = "Assistant";

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
