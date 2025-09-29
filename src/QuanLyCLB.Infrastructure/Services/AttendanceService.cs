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

public class AttendanceService : IAttendanceService
{
    private readonly ClubManagementDbContext _dbContext;

    public AttendanceService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AttendanceRecordDto> CheckInAsync(CheckInRequest request, CancellationToken cancellationToken = default)
    {
        var schedule = await _dbContext.ClassSchedules
            .Include(s => s.TrainingClass)
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == request.ClassScheduleId, cancellationToken)
            ?? throw new InvalidOperationException("Class schedule not found");

        if (schedule.TrainingClass?.InstructorId != request.InstructorId)
        {
            throw new InvalidOperationException("Instructor is not assigned to this class");
        }

        schedule.EnsureScheduleIsActiveForDate(request.CheckedInAt);

        var branch = schedule.Branch ?? throw new InvalidOperationException("Schedule does not reference a branch");
        var distance = GeoDistanceCalculator.CalculateDistanceMeters(branch.Latitude, branch.Longitude, request.Latitude, request.Longitude);
        if (distance > branch.AllowedRadiusMeters)
        {
            throw new InvalidOperationException($"Check-in location is outside of allowed radius ({distance:F2}m > {branch.AllowedRadiusMeters:F2}m)");
        }

        var status = DetermineAttendanceStatus(schedule, request.CheckedInAt);

        var record = new AttendanceRecord
        {
            ClassScheduleId = request.ClassScheduleId,
            InstructorId = request.InstructorId,
            CheckedInAt = request.CheckedInAt,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = status,
            CreatedByUserId = request.InstructorId
        };

        _dbContext.AttendanceRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record.ToDto();
    }

    public async Task<AttendanceRecordDto> CreateManualAttendanceAsync(ManualAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var schedule = await _dbContext.ClassSchedules
            .Include(s => s.TrainingClass)
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == request.ClassScheduleId, cancellationToken)
            ?? throw new InvalidOperationException("Class schedule not found");

        var branch = schedule.Branch ?? throw new InvalidOperationException("Schedule does not reference a branch");

        var record = new AttendanceRecord
        {
            ClassScheduleId = request.ClassScheduleId,
            InstructorId = request.InstructorId,
            CheckedInAt = request.OccurredAt,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            Status = request.Status,
            Notes = request.Notes,
            TicketId = request.TicketId
        };

        _dbContext.AttendanceRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record.ToDto();
    }

    public async Task<IReadOnlyCollection<AttendanceRecordDto>> GetAttendanceByInstructorAsync(Guid instructorId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.InstructorId == instructorId &&
                        r.CheckedInAt >= fromDate.ToDateTime(TimeOnly.MinValue) &&
                        r.CheckedInAt <= toDate.ToDateTime(TimeOnly.MaxValue))
            .OrderBy(r => r.CheckedInAt)
            .Select(r => new AttendanceRecord
            {
                Id = r.Id,
                ClassScheduleId = r.ClassScheduleId,
                InstructorId = r.InstructorId,
                CheckedInAt = r.CheckedInAt,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Status = r.Status,
                Notes = r.Notes,
                TicketId = r.TicketId
            })
            .ToListAsync(cancellationToken);

        return records.Select(r => r.ToDto()).ToList();
    }

    public async Task<AttendanceTicketDto> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var scheduleExists = await _dbContext.ClassSchedules.AnyAsync(s => s.Id == request.ClassScheduleId, cancellationToken);
        if (!scheduleExists)
        {
            throw new InvalidOperationException("Class schedule not found");
        }

        var ticket = new AttendanceTicket
        {
            ClassScheduleId = request.ClassScheduleId,
            InstructorId = request.InstructorId,
            Reason = request.Reason,
            CreatedBy = request.CreatedBy,
            CreatedByUserId = request.CreatedByUserId,
            IsApproved = false
        };

        _dbContext.AttendanceTickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ticket.ToDto();
    }

    public async Task<AttendanceTicketDto?> ApproveTicketAsync(Guid ticketId, TicketApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.AttendanceTickets.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        ticket.IsApproved = request.Approve;
        ticket.ApprovedBy = request.Approver;
        ticket.ApprovedAt = DateTime.UtcNow;
        ticket.UpdatedAt = ticket.ApprovedAt;
        ticket.UpdatedByUserId = request.UpdatedByUserId;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var attendance = await _dbContext.AttendanceRecords.FirstOrDefaultAsync(a => a.TicketId == ticketId, cancellationToken);
            if (attendance is not null)
            {
                attendance.Notes = request.Notes;
                attendance.UpdatedAt = DateTime.UtcNow;
                attendance.UpdatedByUserId = request.UpdatedByUserId;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ticket.ToDto();
    }

    private static AttendanceStatus DetermineAttendanceStatus(ClassSchedule schedule, DateTime checkInTime)
    {
        var checkInDate = DateOnly.FromDateTime(checkInTime);
        var scheduledStart = checkInDate.ToDateTime(schedule.StartTime);
        var gracePeriod = scheduledStart.AddMinutes(10);

        return checkInTime <= gracePeriod ? AttendanceStatus.Present : AttendanceStatus.Late;
    }
}

internal static class ScheduleValidationExtensions
{
    public static void EnsureScheduleIsActiveForDate(this ClassSchedule schedule, DateTime checkInTime)
    {
        if (schedule.TrainingClass is null)
        {
            throw new InvalidOperationException("Schedule is not linked to a class");
        }

        var checkInDate = DateOnly.FromDateTime(checkInTime);
        if (schedule.DayOfWeek != checkInDate.DayOfWeek)
        {
            throw new InvalidOperationException("Check-in day does not match schedule");
        }

        if (checkInDate < schedule.TrainingClass.StartDate)
        {
            throw new InvalidOperationException("Check-in date is before the class start date");
        }

        if (schedule.TrainingClass.EndDate.HasValue && checkInDate > schedule.TrainingClass.EndDate.Value)
        {
            throw new InvalidOperationException("Check-in date is after the class end date");
        }
    }
}

internal static class GeoDistanceCalculator
{
    private const double EarthRadiusMeters = 6371000;

    public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        double ToRadians(double angle) => Math.PI * angle / 180.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Pow(Math.Sin(dLon / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }
}
