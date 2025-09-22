namespace QuanLyClb.Domain.Entities;

public class TuitionPayment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public Guid ClassId { get; set; }
    public TrainingClass Class { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public Guid? CollectedById { get; set; }
    public User? CollectedBy { get; set; }
    public string? Notes { get; set; }
}
