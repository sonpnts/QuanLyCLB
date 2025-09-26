using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Application.Mappings;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class InstructorService : IInstructorService
{
    private readonly ClubManagementDbContext _dbContext;

    public InstructorService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var instructors = await _dbContext.Instructors
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return instructors.Select(x => x.ToDto()).ToList();
    }

    public async Task<InstructorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return instructor?.ToDto();
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Instructors.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (existing)
        {
            throw new InvalidOperationException($"Instructor with email {request.Email} already exists");
        }

        var instructor = new Instructor
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            HourlyRate = request.HourlyRate
        };

        _dbContext.Instructors.Add(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return instructor.ToDto();
    }

    public async Task<InstructorDto?> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (instructor is null)
        {
            return null;
        }

        instructor.FullName = request.FullName;
        instructor.PhoneNumber = request.PhoneNumber;
        instructor.HourlyRate = request.HourlyRate;
        instructor.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return instructor.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (instructor is null)
        {
            return false;
        }

        _dbContext.Instructors.Remove(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<InstructorDto> SyncGoogleAccountAsync(string email, string fullName, string googleSubject, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (instructor is null)
        {
            instructor = new Instructor
            {
                Email = email,
                FullName = fullName,
                GoogleSubject = googleSubject,
                PhoneNumber = string.Empty,
                HourlyRate = 0,
                IsActive = true
            };
            _dbContext.Instructors.Add(instructor);
        }
        else
        {
            instructor.GoogleSubject = googleSubject;
            instructor.FullName = fullName;
            instructor.IsActive = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return instructor.ToDto();
    }
}
