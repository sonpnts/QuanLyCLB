namespace QuanLyCLB.API.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public StudentDto Student { get; set; } = null!;
        public ClassDto Class { get; set; } = null!;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateEnrollmentDto
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class TransferStudentDto
    {
        public int StudentId { get; set; }
        public int FromClassId { get; set; }
        public int ToClassId { get; set; }
        public string? Notes { get; set; }
    }
}