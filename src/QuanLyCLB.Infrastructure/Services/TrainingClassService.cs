using System;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Application.Mappings;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class TrainingClassService : ITrainingClassService
{
    private readonly ClubManagementDbContext _dbContext;

    public TrainingClassService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TrainingClassDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var classes = await _dbContext.TrainingClasses
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return classes.Select(x => x.ToDto()).ToList();
    }

    public async Task<TrainingClassDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trainingClass = await _dbContext.TrainingClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return trainingClass?.ToDto();
    }

    public async Task<TrainingClassDto> CreateAsync(CreateTrainingClassRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.TrainingClasses.AnyAsync(x => x.Code == request.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Class with code {request.Code} already exists");
        }

        var instructorExists = await _dbContext.Instructors.FirstOrDefaultAsync(x => x.UserAccountId == request.InstructorId, cancellationToken);
        if (instructorExists == null)
        {
            throw new InvalidOperationException("Instructor does not exist");
        }

        var entity = new TrainingClass
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MaxStudents = request.MaxStudents,
            InstructorId = instructorExists.Id
        };

        _dbContext.TrainingClasses.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<TrainingClassDto?> UpdateAsync(Guid id, UpdateTrainingClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.TrainingClasses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var instructorExists = await _dbContext.Instructors.AnyAsync(x => x.Id == request.InstructorId, cancellationToken);
        if (!instructorExists)
        {
            throw new InvalidOperationException("Instructor does not exist");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.MaxStudents = request.MaxStudents;
        entity.InstructorId = request.InstructorId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.TrainingClasses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.TrainingClasses.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
