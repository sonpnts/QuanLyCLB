namespace QuanLyCLB.API.DTOs
{
    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxStudents { get; set; }
        public decimal FeePerMonth { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public UserDto Trainer { get; set; } = null!;
        public UserDto? Assistant { get; set; }
        public int CurrentStudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateClassDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxStudents { get; set; } = 20;
        public decimal FeePerMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TrainerId { get; set; }
        public int? AssistantId { get; set; }
    }

    public class UpdateClassDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? MaxStudents { get; set; }
        public decimal? FeePerMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TrainerId { get; set; }
        public int? AssistantId { get; set; }
        public int? Status { get; set; }
    }
}