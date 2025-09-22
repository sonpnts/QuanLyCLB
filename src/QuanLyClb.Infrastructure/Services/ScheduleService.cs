using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Extensions;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _dbContext;

    public ScheduleService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ClassScheduleDto>> UpsertAsync(UpsertScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var trainingClass = await _dbContext.Classes.Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (trainingClass is null)
        {
            throw new KeyNotFoundException($"Class {request.ClassId} not found");
        }

        _dbContext.ClassSchedules.RemoveRange(trainingClass.Schedules);
        trainingClass.Schedules.Clear();

        foreach (var item in request.Items)
        {
            trainingClass.Schedules.Add(new Domain.Entities.ClassSchedule
            {
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Location = item.Location
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(trainingClass).Collection(c => c.Schedules).LoadAsync(cancellationToken);
        return trainingClass.Schedules.Select(s => s.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<ClassScheduleDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        var schedules = await _dbContext.ClassSchedules
            .Where(s => s.ClassId == classId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return schedules.Select(s => s.ToDto()).ToList();
    }
}
