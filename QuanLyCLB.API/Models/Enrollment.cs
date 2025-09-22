using System.ComponentModel.DataAnnotations;

namespace QuanLyCLB.API.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
    }

    public enum EnrollmentStatus
    {
        Active = 1,
        Completed = 2,
        Transferred = 3,
        Dropped = 4
    }
}