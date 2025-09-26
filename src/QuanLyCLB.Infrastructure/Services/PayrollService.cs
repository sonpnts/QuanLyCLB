using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Enums;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Application.Mappings;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly ClubManagementDbContext _dbContext;

    public PayrollService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PayrollPeriodDto> GeneratePayrollAsync(GeneratePayrollRequest request, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(i => i.Id == request.InstructorId, cancellationToken)
            ?? throw new InvalidOperationException("Instructor not found");

        var attendanceRecords = await _dbContext.AttendanceRecords
            .Include(a => a.ClassSchedule)
            .Where(a => a.InstructorId == request.InstructorId &&
                        a.ClassSchedule != null &&
                        a.ClassSchedule.StudyDate.Year == request.Year &&
                        a.ClassSchedule.StudyDate.Month == request.Month &&
                        (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late || a.Status == AttendanceStatus.Manual))
            .ToListAsync(cancellationToken);

        var details = new List<PayrollDetail>();
        decimal totalHours = 0;
        foreach (var attendance in attendanceRecords)
        {
            if (attendance.ClassSchedule is null)
            {
                continue;
            }

            var duration = attendance.ClassSchedule.EndTime - attendance.ClassSchedule.StartTime;
            var hours = (decimal)duration.TotalHours;
            var amount = hours * instructor.HourlyRate;

            totalHours += hours;

            details.Add(new PayrollDetail
            {
                AttendanceRecordId = attendance.Id,
                Hours = hours,
                Amount = amount
            });
        }

        var totalAmount = details.Sum(d => d.Amount);

        var payroll = await _dbContext.PayrollPeriods
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.InstructorId == request.InstructorId && p.Year == request.Year && p.Month == request.Month, cancellationToken);

        if (payroll is null)
        {
            payroll = new PayrollPeriod
            {
                InstructorId = request.InstructorId,
                Year = request.Year,
                Month = request.Month
            };
            _dbContext.PayrollPeriods.Add(payroll);
        }
        else
        {
            _dbContext.PayrollDetails.RemoveRange(payroll.Details);
            payroll.Details.Clear();
        }

        payroll.TotalHours = totalHours;
        payroll.TotalAmount = totalAmount;
        payroll.GeneratedAt = DateTime.UtcNow;
        payroll.Details = details;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(payroll).Collection(p => p.Details).LoadAsync(cancellationToken);

        return payroll.ToDto();
    }

    public async Task<IReadOnlyCollection<PayrollPeriodDto>> GetPayrollsAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var payrolls = await _dbContext.PayrollPeriods
            .Include(p => p.Details)
            .Where(p => p.InstructorId == instructorId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);

        return payrolls.Select(p => p.ToDto()).ToList();
    }

    public async Task<PayrollPeriodDto?> GetPayrollByIdAsync(Guid payrollId, CancellationToken cancellationToken = default)
    {
        var payroll = await _dbContext.PayrollPeriods
            .Include(p => p.Details)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == payrollId, cancellationToken);

        return payroll?.ToDto();
    }
}
