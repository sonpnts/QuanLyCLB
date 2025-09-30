using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        var coach = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(i => i.Id == request.CoachId, cancellationToken)
            ?? throw new InvalidOperationException("Coach not found");

        if (!coach.UserRoles.Any(r => r.Role.Name == "Coach"))
        {
            throw new InvalidOperationException("User does not have the Coach role");
        }

        var payrollRule = await _dbContext.PayrollRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleName == "Coach" && r.SkillLevel == coach.SkillLevel, cancellationToken)
            ?? throw new InvalidOperationException("Payroll rule not configured for coach skill level");

        var monthStart = DateTime.SpecifyKind(new DateTime(request.Year, request.Month, 1, 0, 0, 0), DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var attendanceRecords = await _dbContext.AttendanceRecords
            .Include(a => a.ClassSchedule)
            .Where(a => a.CoachId == request.CoachId &&
                        a.CheckedInAt >= monthStart &&
                        a.CheckedInAt <= monthEnd &&
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
            var amount = hours * payrollRule.HourlyRate;

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
            .FirstOrDefaultAsync(p => p.CoachId == request.CoachId && p.Year == request.Year && p.Month == request.Month, cancellationToken);

        if (payroll is null)
        {
            payroll = new PayrollPeriod
            {
                CoachId = request.CoachId,
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

    public async Task<IReadOnlyCollection<PayrollPeriodDto>> GetPayrollsAsync(Guid coachId, CancellationToken cancellationToken = default)
    {
        var payrolls = await _dbContext.PayrollPeriods
            .Include(p => p.Details)
            .Where(p => p.CoachId == coachId)
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
