namespace QuanLyCLB.API.DTOs
{
    public class StudentReportDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int TransferredStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public List<StudentDto> Students { get; set; } = new();
    }

    public class ClassReportDto
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public int InactiveClasses { get; set; }
        public int CompletedClasses { get; set; }
        public int ArchivedClasses { get; set; }
        public List<ClassDto> Classes { get; set; } = new();
    }

    public class AttendanceReportDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalSessions { get; set; }
        public int TotalStudents { get; set; }
        public double AverageAttendanceRate { get; set; }
        public List<StudentAttendanceStatsDto> StudentStats { get; set; } = new();
    }

    public class StudentAttendanceStatsDto
    {
        public StudentDto Student { get; set; } = null!;
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class FinancialReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyFeesRevenue { get; set; }
        public decimal RegistrationFeesRevenue { get; set; }
        public decimal OtherRevenue { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public int TotalPayments { get; set; }
        public int PaidPayments { get; set; }
        public int PendingPayments { get; set; }
        public int OverduePayments { get; set; }
        public List<ClassRevenueDto> ClassRevenues { get; set; } = new();
    }

    public class ClassRevenueDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
    }

    public class DashboardDto
    {
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalTrainers { get; set; }
        public int TotalAssistants { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public int OverduePaymentsCount { get; set; }
        public double AverageAttendanceRate { get; set; }
        public List<ClassDto> RecentClasses { get; set; } = new();
        public List<StudentDto> RecentStudents { get; set; } = new();
        public List<PaymentDto> RecentPayments { get; set; } = new();
    }
}