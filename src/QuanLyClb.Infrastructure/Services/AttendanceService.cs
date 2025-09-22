using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Extensions;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _dbContext;

    public AttendanceService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AttendanceSessionDto> CreateSessionAsync(CreateAttendanceSessionRequest request, Guid markedById, CancellationToken cancellationToken = default)
    {
        var trainingClass = await _dbContext.Classes.Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (trainingClass is null)
        {
            throw new KeyNotFoundException($"Class {request.ClassId} not found");
        }

        var session = new Domain.Entities.AttendanceSession
        {
            ClassId = trainingClass.Id,
            SessionDate = request.SessionDate,
            CoachId = request.CoachId,
            MarkedById = markedById,
            PhotoUrl = request.PhotoUrl,
            Notes = request.Notes
        };

        foreach (var enrollment in trainingClass.Enrollments.Where(e => e.Status == Domain.Enums.EnrollmentStatus.Active))
        {
            session.Records.Add(new Domain.Entities.AttendanceRecord
            {
                StudentId = enrollment.StudentId,
                Status = Domain.Enums.AttendanceStatus.Present
            });
        }

        _dbContext.AttendanceSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(session).Collection(s => s.Records).LoadAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<AttendanceSessionDto> MarkAsync(MarkAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.AttendanceSessions
            .Include(s => s.Records)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
        if (session is null)
        {
            throw new KeyNotFoundException($"Attendance session {request.SessionId} not found");
        }

        var record = session.Records.FirstOrDefault(r => r.StudentId == request.StudentId);
        if (record is null)
        {
            record = new Domain.Entities.AttendanceRecord
            {
                StudentId = request.StudentId,
                AttendanceSessionId = session.Id
            };
            session.Records.Add(record);
        }

        record.Status = request.Status;
        record.MarkedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<IReadOnlyCollection<AttendanceSessionDto>> GetSessionsAsync(Guid classId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AttendanceSessions
            .Include(s => s.Records)
            .Where(s => s.ClassId == classId)
            .AsNoTracking();

        if (from.HasValue)
        {
            query = query.Where(s => s.SessionDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.SessionDate <= to.Value);
        }

        var sessions = await query.OrderByDescending(s => s.SessionDate).ToListAsync(cancellationToken);
        return sessions.Select(s => s.ToDto()).ToList();
    }
}
