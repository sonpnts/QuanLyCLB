using System.ComponentModel.DataAnnotations;

namespace QuanLyCLB.API.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int ClassId { get; set; }
        public int UserId { get; set; } // Trainer or Assistant
        
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        [StringLength(255)]
        public string? Location { get; set; }
        
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Class Class { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}