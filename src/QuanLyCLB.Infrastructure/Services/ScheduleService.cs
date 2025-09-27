using System;
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
            .Where(x => x.TrainingClassId == classId)
            .OrderBy(x => x.StudyDate)
            .ToListAsync(cancellationToken);

        return schedules.Select(x => x.ToDto()).ToList();
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClassSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureClassExistsAsync(request.TrainingClassId, cancellationToken);

        var entity = new ClassSchedule
        {
            TrainingClassId = request.TrainingClassId,
            StudyDate = request.StudyDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DayOfWeek = request.DayOfWeek,
            LocationName = request.LocationName,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AllowedRadiusMeters = request.AllowedRadiusMeters
        };

        _dbContext.ClassSchedules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<IReadOnlyCollection<ClassScheduleDto>> BulkCreateAsync(BulkCreateScheduleRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FromDate > request.ToDate)
        {
            throw new ArgumentException("FromDate must be before ToDate");
        }

        await EnsureClassExistsAsync(request.TrainingClassId, cancellationToken);

        var schedules = new List<ClassSchedule>();
        for (var date = request.FromDate; date <= request.ToDate; date = date.AddDays(1))
        {
            if (!request.DaysOfWeek.Contains(date.DayOfWeek))
            {
                continue;
            }

            var entity = new ClassSchedule
            {
                TrainingClassId = request.TrainingClassId,
                StudyDate = date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                DayOfWeek = date.DayOfWeek,
                LocationName = request.LocationName,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AllowedRadiusMeters = request.AllowedRadiusMeters
            };
            schedules.Add(entity);
        }

        await _dbContext.ClassSchedules.AddRangeAsync(schedules, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return schedules.Select(x => x.ToDto()).ToList();
    }

    public async Task<ClassScheduleDto?> UpdateAsync(Guid id, UpdateClassScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClassSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.StudyDate = request.StudyDate;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.DayOfWeek = request.DayOfWeek;
        entity.LocationName = request.LocationName;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.AllowedRadiusMeters = request.AllowedRadiusMeters;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClassSchedules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.ClassSchedules.Remove(entity);
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
}
