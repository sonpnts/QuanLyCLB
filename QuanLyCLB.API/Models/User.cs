using System.ComponentModel.DataAnnotations;

namespace QuanLyCLB.API.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        public string? GoogleId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Class> ClassesAsTrainer { get; set; } = new List<Class>();
        public ICollection<Class> ClassesAsAssistant { get; set; } = new List<Class>();
        public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }

    public enum UserRole
    {
        Admin = 1,
        Trainer = 2,
        Assistant = 3
    }
}