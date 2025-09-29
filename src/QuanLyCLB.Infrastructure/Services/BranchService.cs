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

public class BranchService : IBranchService
{
    private readonly ClubManagementDbContext _dbContext;

    public BranchService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var branches = await _dbContext.Branches
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        return branches.Select(b => b.ToDto()).ToList();
    }

    public async Task<BranchDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return branch?.ToDto();
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Branch
        {
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AllowedRadiusMeters = request.AllowedRadiusMeters,
            GooglePlaceId = request.GooglePlaceId,
            GoogleMapsEmbedUrl = request.GoogleMapsEmbedUrl
        };

        _dbContext.Branches.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<BranchDto?> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name;
        entity.Address = request.Address;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.AllowedRadiusMeters = request.AllowedRadiusMeters;
        entity.GooglePlaceId = request.GooglePlaceId;
        entity.GoogleMapsEmbedUrl = request.GoogleMapsEmbedUrl;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Branches
            .Include(b => b.ClassSchedules)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.ClassSchedules.Any())
        {
            throw new InvalidOperationException("Cannot delete branch while schedules are still assigned");
        }

        _dbContext.Branches.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
