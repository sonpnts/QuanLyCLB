using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _dbContext;

    public ReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TuitionReportItemDto>> GetTuitionReportAsync(TuitionReportRequest request, CancellationToken cancellationToken = default)
    {
        var query = await _dbContext.TuitionPayments
            .Where(p => p.PaidAt >= request.From && p.PaidAt <= request.To)
            .GroupBy(p => new { p.ClassId, p.Class.Name })
            .Select(g => new TuitionReportItemDto(
                g.Key.ClassId,
                g.Key.Name,
                g.Sum(p => p.Amount),
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        return query;
    }

    public async Task<ClassRosterDto?> GetClassRosterAsync(ClassRosterRequest request, CancellationToken cancellationToken = default)
    {
        var trainingClass = await _dbContext.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (trainingClass is null)
        {
            return null;
        }

        var students = await _dbContext.Enrollments
            .Where(e => e.ClassId == request.ClassId && e.Status == Domain.Enums.EnrollmentStatus.Active)
            .Select(e => new ClassRosterItemDto(
                e.StudentId,
                e.Student.FullName,
                e.Student.Email,
                e.Student.PhoneNumber,
                e.Student.DateOfBirth
            ))
            .ToListAsync(cancellationToken);

        return new ClassRosterDto(trainingClass.Id, trainingClass.Name, students);
    }
}
