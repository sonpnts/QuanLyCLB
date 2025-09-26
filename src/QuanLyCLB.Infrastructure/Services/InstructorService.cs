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
            .Include(x => x.User)
            .OrderBy(x => x.User.FullName)
            .ToListAsync(cancellationToken);

        return instructors.Select(x => x.ToDto()).ToList();
    }

    public async Task<InstructorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return instructor?.ToDto();
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Instructor)
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user is not null && user.Instructor is not null)
        {
            throw new InvalidOperationException($"Instructor with email {request.Email} already exists");
        }

        if (user is null)
        {
            user = new UserAccount
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };
            _dbContext.Users.Add(user);
        }
        else
        {
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = true;
        }

        var instructor = new Instructor
        {
            User = user,
            HourlyRate = request.HourlyRate,
            IsActive = true
        };

        _dbContext.Instructors.Add(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(instructor).Reference(x => x.User).LoadAsync(cancellationToken);

        return instructor.ToDto();
    }

    public async Task<InstructorDto?> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (instructor is null)
        {
            return null;
        }

        instructor.User.FullName = request.FullName;
        instructor.User.PhoneNumber = request.PhoneNumber;
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
        var user = await _dbContext.Users
            .Include(u => u.Instructor)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user?.Instructor is null)
        {
            throw new InvalidOperationException("Instructor is not registered in the system");
        }

        if (!user.IsActive || !user.Instructor.IsActive)
        {
            throw new InvalidOperationException("Instructor account is disabled");
        }

        if (string.IsNullOrWhiteSpace(user.GoogleSubject))
        {
            user.GoogleSubject = googleSubject;
        }
        else if (!string.Equals(user.GoogleSubject, googleSubject, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Google account does not match the registered instructor");
        }

        user.FullName = fullName;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var instructorEntity = user.Instructor;
        instructorEntity.User = user;

        return instructorEntity.ToDto();
    }
}
