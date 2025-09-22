namespace QuanLyCLB.API.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public ClassDto Class { get; set; } = null!;
        public UserDto User { get; set; } = null!;
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateScheduleDto
    {
        public int ClassId { get; set; }
        public int UserId { get; set; }
        public int DayOfWeek { get; set; } // DayOfWeek as int (0=Sunday, 1=Monday, etc.)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateScheduleDto
    {
        public int? UserId { get; set; }
        public int? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Location { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool? IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class WeeklyScheduleDto
    {
        public DateTime WeekStartDate { get; set; }
        public List<DayScheduleDto> Days { get; set; } = new();
    }

    public class DayScheduleDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new();
    }
}