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
        var emailExists = await _dbContext.Instructors
            .AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException($"Instructor with email {request.Email} already exists");
        }

        var userAccount = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (userAccount is null)
        {
            userAccount = new UserAccount
            {
                Username = request.Email,
                Email = request.Email,
                IsActive = true
            };
            _dbContext.Users.Add(userAccount);
        }
        else
        {
            if (!userAccount.IsActive)
            {
                throw new InvalidOperationException("User account is disabled");
            }

            userAccount.Username = request.Email;
            userAccount.Email = request.Email;
        }

        var instructor = new Instructor
        {
            UserAccountId = userAccount.Id,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            HourlyRate = request.HourlyRate,
            IsActive = true
        };

        _dbContext.Instructors.Add(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return instructor.ToDto();
    }

    public async Task<InstructorDto?> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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
        var userAccount = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (userAccount is null)
        {
            throw new InvalidOperationException("Email is not registered in the system");
        }

        if (!userAccount.IsActive)
        {
            throw new InvalidOperationException("User account is disabled");
        }

        if (string.IsNullOrWhiteSpace(userAccount.GoogleSubject))
        {
            userAccount.GoogleSubject = googleSubject;
        }
        else if (!string.Equals(userAccount.GoogleSubject, googleSubject, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Google account does not match the registered email");
        }

        userAccount.Username = email;
        userAccount.Email = email;

        var instructor = await _dbContext.Instructors
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (instructor is null)
        {
            throw new InvalidOperationException("Instructor is not registered in the system");
        }

        if (!instructor.IsActive)
        {
            throw new InvalidOperationException("Instructor account is disabled");
        }

        instructor.FullName = fullName;
        instructor.UserAccountId ??= userAccount.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return instructor.ToDto();
    }
}
