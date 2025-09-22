using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class TrainingClass : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Capacity { get; set; }
    public ClassStatus Status { get; set; } = ClassStatus.Draft;
    public bool IsArchived { get; set; }
    public Guid? CoachId { get; set; }
    public User? Coach { get; set; }
    public Guid? AssistantId { get; set; }
    public User? Assistant { get; set; }
    public Guid? ParentClassId { get; set; }
    public TrainingClass? ParentClass { get; set; }

    public ICollection<TrainingClass> ClonedClasses { get; set; } = new List<TrainingClass>();
    public ICollection<ClassSchedule> Schedules { get; set; } = new List<ClassSchedule>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
}
