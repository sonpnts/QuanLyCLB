using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class Student : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Prospect;
    public string? Notes { get; set; }

    public Guid? CurrentClassId { get; set; }
    public TrainingClass? CurrentClass { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<TuitionPayment> Payments { get; set; } = new List<TuitionPayment>();
}
