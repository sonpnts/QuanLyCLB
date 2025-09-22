using System.ComponentModel.DataAnnotations;

namespace QuanLyCLB.API.Models
{
    public class Class
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int MaxStudents { get; set; } = 20;
        
        public decimal FeePerMonth { get; set; }
        
        public ClassStatus Status { get; set; } = ClassStatus.Active;
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Foreign keys
        public int TrainerId { get; set; }
        public int? AssistantId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public User Trainer { get; set; } = null!;
        public User? Assistant { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }

    public enum ClassStatus
    {
        Active = 1,
        Inactive = 2,
        Completed = 3,
        Archived = 4
    }
}