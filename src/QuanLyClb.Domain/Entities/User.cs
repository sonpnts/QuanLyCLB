using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public string? GoogleId { get; set; }

    public ICollection<TrainingClass> CoachingClasses { get; set; } = new List<TrainingClass>();
    public ICollection<TrainingClass> AssistingClasses { get; set; } = new List<TrainingClass>();
    public ICollection<TuitionPayment> CollectedPayments { get; set; } = new List<TuitionPayment>();
}
