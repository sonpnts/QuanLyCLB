using System.ComponentModel.DataAnnotations;

namespace QuanLyCLB.API.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int RecordedById { get; set; } // User who recorded attendance
        
        public DateTime AttendanceDate { get; set; }
        public AttendanceStatus Status { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(255)]
        public string? PhotoUrl { get; set; } // URL to attendance photo
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
        public User RecordedBy { get; set; } = null!;
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        Excused = 4
    }
}