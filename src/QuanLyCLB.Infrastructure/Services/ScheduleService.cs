using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Application.Mappings;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly ClubManagementDbContext _dbContext;

    public ScheduleService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ClassScheduleDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        var schedules = await _dbContext.ClassSchedules
            .AsNoTracking()
            .Include(x => x.Branch)
            .Where(x => x.TrainingClassId == classId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        return schedules.Select(x => x.ToDto()).ToList();
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClassSchedules
            .AsNoTracking()
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureClassExistsAsync(request.TrainingClassId, cancellationToken);
        await EnsureBranchExistsAsync(request.BranchId, cancellationToken);

        await EnsureScheduleUniquenessAsync(request.TrainingClassId, request.DayOfWeek, null, cancellationToken);

        var entity = new ClassSchedule
        {
            TrainingClassId = request.TrainingClassId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BranchId = request.BranchId
        };

        _dbContext.ClassSchedules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Entry(entity).Reference(e => e.Branch).LoadAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<IReadOnlyCollection<ClassScheduleDto>> BulkCreateAsync(BulkCreateScheduleRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureClassExistsAsync(request.TrainingClassId, cancellationToken);
        await EnsureBranchExistsAsync(request.BranchId, cancellationToken);

        if (request.DaysOfWeek is null)
        {
            throw new ArgumentException("DaysOfWeek is required");
        }

        var distinctDays = request.DaysOfWeek.Distinct().ToList();
        if (!distinctDays.Any())
        {
            throw new InvalidOperationException("At least one day of week must be specified");
        }
        var existingSchedules = await _dbContext.ClassSchedules
            .Where(x => x.TrainingClassId == request.TrainingClassId && distinctDays.Contains(x.DayOfWeek))
            .ToListAsync(cancellationToken);

        foreach (var day in distinctDays)
        {
            await EnsureScheduleUniquenessAsync(request.TrainingClassId, day, existingSchedules.FirstOrDefault(x => x.DayOfWeek == day)?.Id, cancellationToken);
        }

        foreach (var day in distinctDays)
        {
            var schedule = existingSchedules.FirstOrDefault(x => x.DayOfWeek == day);
            if (schedule is null)
            {
                schedule = new ClassSchedule
                {
                    TrainingClassId = request.TrainingClassId,
                    DayOfWeek = day,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    BranchId = request.BranchId
                };
                _dbContext.ClassSchedules.Add(schedule);
                existingSchedules.Add(schedule);
            }
            else
            {
                schedule.StartTime = request.StartTime;
                schedule.EndTime = request.EndTime;
                schedule.BranchId = request.BranchId;
                schedule.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var scheduleIds = existingSchedules.Where(x => distinctDays.Contains(x.DayOfWeek)).Select(x => x.Id).ToList();
        var reloaded = await _dbContext.ClassSchedules
            .AsNoTracking()
            .Include(x => x.Branch)
            .Where(x => scheduleIds.Contains(x.Id))
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        return reloaded.Select(x => x.ToDto()).ToList();
    }

    public async Task<ClassScheduleDto?> UpdateAsync(Guid id, UpdateClassScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClassSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await EnsureBranchExistsAsync(request.BranchId, cancellationToken);
        await EnsureScheduleUniquenessAsync(entity.TrainingClassId, request.DayOfWeek, entity.Id, cancellationToken);

        entity.DayOfWeek = request.DayOfWeek;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.BranchId = request.BranchId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(entity).Reference(e => e.Branch).LoadAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClassSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureClassExistsAsync(Guid classId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.TrainingClasses.AnyAsync(x => x.Id == classId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Training class does not exist");
        }
    }

    private async Task EnsureBranchExistsAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Branches.AnyAsync(x => x.Id == branchId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Branch does not exist");
        }
    }

    private async Task EnsureScheduleUniquenessAsync(Guid classId, DayOfWeek dayOfWeek, Guid? currentScheduleId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ClassSchedules.AnyAsync(
            x => x.TrainingClassId == classId &&
                 x.DayOfWeek == dayOfWeek &&
                 (!currentScheduleId.HasValue || x.Id != currentScheduleId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Schedule for {dayOfWeek} already exists for this class");
        }
    }
}
