using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Extensions;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Entities;
using QuanLyClb.Domain.Enums;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class ClassService : IClassService
{
    private readonly ApplicationDbContext _dbContext;

    public ClassService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TrainingClass
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            Status = request.Status,
            CoachId = request.CoachId,
            AssistantId = request.AssistantId
        };

        _dbContext.Classes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ClassDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<IReadOnlyCollection<ClassDto>> SearchAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        keyword = keyword?.Trim().ToLowerInvariant();
        var query = _dbContext.Classes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c => c.Name.ToLower().Contains(keyword));
        }

        var classes = await query.OrderBy(c => c.StartDate).ToListAsync(cancellationToken);
        return classes.Select(c => c.ToDto()).ToList();
    }

    public async Task<ClassDto> UpdateAsync(UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Class {request.Id} not found");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Capacity = request.Capacity;
        entity.Status = request.Status;
        entity.CoachId = request.CoachId;
        entity.AssistantId = request.AssistantId;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ClassDto> CloneAsync(CloneClassRequest request, CancellationToken cancellationToken = default)
    {
        var source = await _dbContext.Classes.Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == request.SourceClassId, cancellationToken);
        if (source is null)
        {
            throw new KeyNotFoundException($"Class {request.SourceClassId} not found");
        }

        var clone = new TrainingClass
        {
            Name = request.NewName,
            Description = source.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = source.Capacity,
            Status = ClassStatus.Scheduled,
            CoachId = source.CoachId,
            AssistantId = source.AssistantId,
            ParentClassId = source.Id
        };

        foreach (var schedule in source.Schedules)
        {
            clone.Schedules.Add(new ClassSchedule
            {
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Location = schedule.Location
            });
        }

        _dbContext.Classes.Add(clone);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return clone.ToDto();
    }

    public async Task ArchiveAsync(ArchiveClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Class {request.ClassId} not found");
        }

        entity.IsArchived = request.IsArchived;
        if (request.IsArchived)
        {
            entity.Status = ClassStatus.Archived;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
