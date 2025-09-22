namespace QuanLyCLB.API.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public StudentDto Student { get; set; } = null!;
        public ClassDto Class { get; set; } = null!;
        public UserDto RecordedBy { get; set; } = null!;
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAttendanceDto
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int Status { get; set; } // AttendanceStatus as int
        public string? Notes { get; set; }
    }

    public class BulkAttendanceDto
    {
        public int ClassId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public List<StudentAttendanceDto> Students { get; set; } = new();
    }

    public class StudentAttendanceDto
    {
        public int StudentId { get; set; }
        public int Status { get; set; } // AttendanceStatus as int
        public string? Notes { get; set; }
    }

    public class AttendanceUploadDto
    {
        public int AttendanceId { get; set; }
        public IFormFile Photo { get; set; } = null!;
    }
}