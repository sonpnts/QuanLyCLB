using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public Guid ClassId { get; set; }
    public TrainingClass Class { get; set; } = default!;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
}
