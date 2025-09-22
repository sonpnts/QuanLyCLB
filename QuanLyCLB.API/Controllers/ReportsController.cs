using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.API.Data;
using QuanLyCLB.API.Models;
using QuanLyCLB.API.DTOs;

namespace QuanLyCLB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;

        public ReportsController(QuanLyCLBContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var monthStart = new DateTime(currentYear, currentMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var totalStudents = await _context.Students.CountAsync();
            var totalClasses = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Active);
            var totalTrainers = await _context.Users.CountAsync(u => u.Role == UserRole.Trainer);
            var totalAssistants = await _context.Users.CountAsync(u => u.Role == UserRole.Assistant);

            var monthlyRevenue = await _context.Payments
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= monthEnd && p.Status == PaymentStatus.Paid)
                .SumAsync(p => p.Amount);

            var pendingPayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending)
                .SumAsync(p => p.Amount);

            var overduePaymentsCount = await _context.Payments
                .CountAsync(p => p.Status == PaymentStatus.Overdue);

            // Calculate average attendance rate for current month
            var attendanceRecords = await _context.Attendances
                .Where(a => a.AttendanceDate >= monthStart && a.AttendanceDate <= monthEnd)
                .ToListAsync();

            var averageAttendanceRate = attendanceRecords.Any() 
                ? (double)attendanceRecords.Count(a => a.Status == AttendanceStatus.Present) / attendanceRecords.Count * 100
                : 0;

            var recentClasses = await _context.Classes
                .Include(c => c.Trainer)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status.ToString(),
                    MaxStudents = c.MaxStudents,
                    FeePerMonth = c.FeePerMonth,
                    StartDate = c.StartDate,
                    Trainer = new UserDto
                    {
                        Id = c.Trainer.Id,
                        FullName = c.Trainer.FullName,
                        Role = c.Trainer.Role.ToString()
                    },
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var recentStudents = await _context.Students
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            var recentPayments = await _context.Payments
                .Include(p => p.Student)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        Status = p.Student.Status.ToString()
                    },
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    DueDate = p.DueDate,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var dashboard = new DashboardDto
            {
                TotalStudents = totalStudents,
                TotalClasses = totalClasses,
                TotalTrainers = totalTrainers,
                TotalAssistants = totalAssistants,
                MonthlyRevenue = monthlyRevenue,
                PendingPayments = pendingPayments,
                OverduePaymentsCount = overduePaymentsCount,
                AverageAttendanceRate = averageAttendanceRate,
                RecentClasses = recentClasses,
                RecentStudents = recentStudents,
                RecentPayments = recentPayments
            };

            return Ok(dashboard);
        }

        [HttpGet("students")]
        public async Task<ActionResult<StudentReportDto>> GetStudentReport()
        {
            var students = await _context.Students
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    Address = s.Address,
                    ParentName = s.ParentName,
                    ParentPhone = s.ParentPhone,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            var report = new StudentReportDto
            {
                TotalStudents = students.Count,
                ActiveStudents = students.Count(s => s.Status == StudentStatus.Active.ToString()),
                InactiveStudents = students.Count(s => s.Status == StudentStatus.Inactive.ToString()),
                TransferredStudents = students.Count(s => s.Status == StudentStatus.Transferred.ToString()),
                GraduatedStudents = students.Count(s => s.Status == StudentStatus.Graduated.ToString()),
                Students = students
            };

            return Ok(report);
        }

        [HttpGet("classes")]
        public async Task<ActionResult<ClassReportDto>> GetClassReport()
        {
            var classes = await _context.Classes
                .Include(c => c.Trainer)
                .Include(c => c.Assistant)
                .Include(c => c.Enrollments)
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    MaxStudents = c.MaxStudents,
                    FeePerMonth = c.FeePerMonth,
                    Status = c.Status.ToString(),
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Trainer = new UserDto
                    {
                        Id = c.Trainer.Id,
                        FullName = c.Trainer.FullName,
                        Role = c.Trainer.Role.ToString()
                    },
                    Assistant = c.Assistant != null ? new UserDto
                    {
                        Id = c.Assistant.Id,
                        FullName = c.Assistant.FullName,
                        Role = c.Assistant.Role.ToString()
                    } : null,
                    CurrentStudentCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var report = new ClassReportDto
            {
                TotalClasses = classes.Count,
                ActiveClasses = classes.Count(c => c.Status == ClassStatus.Active.ToString()),
                InactiveClasses = classes.Count(c => c.Status == ClassStatus.Inactive.ToString()),
                CompletedClasses = classes.Count(c => c.Status == ClassStatus.Completed.ToString()),
                ArchivedClasses = classes.Count(c => c.Status == ClassStatus.Archived.ToString()),
                Classes = classes
            };

            return Ok(report);
        }

        [HttpGet("attendance/{classId}")]
        public async Task<ActionResult<AttendanceReportDto>> GetAttendanceReport(int classId, DateTime fromDate, DateTime toDate)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
            {
                return NotFound("Class not found");
            }

            var attendanceRecords = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.ClassId == classId && a.AttendanceDate >= fromDate && a.AttendanceDate <= toDate)
                .ToListAsync();

            var enrolledStudents = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.Student)
                .ToListAsync();

            var uniqueDates = attendanceRecords.Select(a => a.AttendanceDate.Date).Distinct().Count();

            var studentStats = enrolledStudents.Select(student =>
            {
                var studentAttendances = attendanceRecords.Where(a => a.StudentId == student.Id).ToList();
                var presentCount = studentAttendances.Count(a => a.Status == AttendanceStatus.Present);
                var absentCount = studentAttendances.Count(a => a.Status == AttendanceStatus.Absent);
                var lateCount = studentAttendances.Count(a => a.Status == AttendanceStatus.Late);
                var excusedCount = studentAttendances.Count(a => a.Status == AttendanceStatus.Excused);
                var totalSessions = studentAttendances.Count;

                return new StudentAttendanceStatsDto
                {
                    Student = new StudentDto
                    {
                        Id = student.Id,
                        FullName = student.FullName,
                        Email = student.Email,
                        Status = student.Status.ToString()
                    },
                    TotalSessions = totalSessions,
                    PresentCount = presentCount,
                    AbsentCount = absentCount,
                    LateCount = lateCount,
                    ExcusedCount = excusedCount,
                    AttendanceRate = totalSessions > 0 ? (double)presentCount / totalSessions * 100 : 0
                };
            }).ToList();

            var averageAttendanceRate = studentStats.Any() 
                ? studentStats.Average(s => s.AttendanceRate) 
                : 0;

            var report = new AttendanceReportDto
            {
                ClassId = classId,
                ClassName = classEntity.Name,
                FromDate = fromDate,
                ToDate = toDate,
                TotalSessions = uniqueDates,
                TotalStudents = enrolledStudents.Count,
                AverageAttendanceRate = averageAttendanceRate,
                StudentStats = studentStats
            };

            return Ok(report);
        }

        [HttpGet("financial")]
        public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(DateTime fromDate, DateTime toDate)
        {
            var payments = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
                .ToListAsync();

            var totalRevenue = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
            var monthlyFeesRevenue = payments.Where(p => p.PaymentType == PaymentType.MonthlyFee && p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
            var registrationFeesRevenue = payments.Where(p => p.PaymentType == PaymentType.RegistrationFee && p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
            var otherRevenue = payments.Where(p => p.PaymentType != PaymentType.MonthlyFee && p.PaymentType != PaymentType.RegistrationFee && p.Status == PaymentStatus.Paid).Sum(p => p.Amount);

            var pendingAmount = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount);
            var overdueAmount = payments.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.Amount);

            var classRevenues = payments
                .Where(p => p.Class != null)
                .GroupBy(p => p.Class)
                .Select(g => new ClassRevenueDto
                {
                    ClassId = g.Key!.Id,
                    ClassName = g.Key.Name,
                    StudentCount = g.Select(p => p.StudentId).Distinct().Count(),
                    TotalRevenue = g.Sum(p => p.Amount),
                    PaidAmount = g.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount),
                    PendingAmount = g.Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue).Sum(p => p.Amount)
                })
                .ToList();

            var report = new FinancialReportDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalRevenue = totalRevenue,
                MonthlyFeesRevenue = monthlyFeesRevenue,
                RegistrationFeesRevenue = registrationFeesRevenue,
                OtherRevenue = otherRevenue,
                PendingAmount = pendingAmount,
                OverdueAmount = overdueAmount,
                TotalPayments = payments.Count,
                PaidPayments = payments.Count(p => p.Status == PaymentStatus.Paid),
                PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending),
                OverduePayments = payments.Count(p => p.Status == PaymentStatus.Overdue),
                ClassRevenues = classRevenues
            };

            return Ok(report);
        }

        [HttpGet("class-students/{classId}")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetClassStudentList(int classId)
        {
            var students = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .Select(e => new StudentDto
                {
                    Id = e.Student.Id,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,
                    PhoneNumber = e.Student.PhoneNumber,
                    DateOfBirth = e.Student.DateOfBirth,
                    Address = e.Student.Address,
                    ParentName = e.Student.ParentName,
                    ParentPhone = e.Student.ParentPhone,
                    Status = e.Student.Status.ToString(),
                    CreatedAt = e.Student.CreatedAt
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("student-fees/{studentId}")]
        public async Task<ActionResult<PaymentReportDto>> GetStudentFeeReport(int studentId)
        {
            var payments = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .Where(p => p.StudentId == studentId)
                .ToListAsync();

            var report = new PaymentReportDto
            {
                FromDate = payments.Any() ? payments.Min(p => p.CreatedAt) : DateTime.UtcNow,
                ToDate = payments.Any() ? payments.Max(p => p.CreatedAt) : DateTime.UtcNow,
                TotalAmount = payments.Sum(p => p.Amount),
                TotalPayments = payments.Count,
                PaidAmount = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount),
                PendingAmount = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                OverdueAmount = payments.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.Amount),
                Payments = payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        Email = p.Student.Email,
                        Status = p.Student.Status.ToString()
                    },
                    Class = p.Class != null ? new ClassDto
                    {
                        Id = p.Class.Id,
                        Name = p.Class.Name,
                        FeePerMonth = p.Class.FeePerMonth,
                        Trainer = new UserDto
                        {
                            Id = p.Class.Trainer.Id,
                            FullName = p.Class.Trainer.FullName,
                            Role = p.Class.Trainer.Role.ToString()
                        }
                    } : null,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    DueDate = p.DueDate,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                }).OrderByDescending(p => p.CreatedAt).ToList()
            };

            return Ok(report);
        }
    }
}