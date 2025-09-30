using System;
using System.Linq;
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

    public async Task<PagedResult<InstructorDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _dbContext.Instructors
            .AsNoTracking()
            .Include(x => x.User)
            .OrderBy(x => x.User.FullName);

        var totalCount = await query.CountAsync(cancellationToken);
        var instructors = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = instructors
            .Select(x => x.ToDto())
            .ToList();

        return new PagedResult<InstructorDto>(items, totalCount, pageNumber, pageSize);
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
        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Instructor)
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (userAccount is null)
        {
            userAccount = new UserAccount
            {
                Username = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
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

            if (userAccount.Instructor is not null)
            {
                throw new InvalidOperationException($"Instructor with email {request.Email} already exists");
            }

            userAccount.Username = request.Email;
            userAccount.Email = request.Email;
            userAccount.FullName = request.FullName;
            userAccount.PhoneNumber = request.PhoneNumber;
        }

        var instructor = new Instructor
        {
            UserAccountId = userAccount.Id,
            HourlyRate = request.HourlyRate,
            IsActive = true
        };

        _dbContext.Instructors.Add(instructor);
        await EnsureRoleAssignedAsync(userAccount, "Instructor", cancellationToken);
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
        instructor.User.IsActive = request.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;
        instructor.User.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return instructor.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (instructor is null)
        {
            return false;
        }

        var instructorRole = instructor.User.UserRoles
            .FirstOrDefault(x => string.Equals(x.Role.Name, "Instructor", StringComparison.OrdinalIgnoreCase));
        if (instructorRole is not null)
        {
            _dbContext.UserRoles.Remove(instructorRole);
        }

        _dbContext.Instructors.Remove(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<InstructorAuthResult> SyncGoogleAccountAsync(string email, string fullName, string googleSubject, CancellationToken cancellationToken = default)
    {
        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
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

        //userAccount.Username = email;
        userAccount.Email = email;
        //userAccount.FullName = fullName;
        userAccount.AvatarUrl = null;

        var instructor = await _dbContext.Instructors
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserAccountId == userAccount.Id, cancellationToken);

        if (instructor is null)
        {
            throw new InvalidOperationException("Instructor is not registered in the system");
        }

        if (!instructor.IsActive)
        {
            throw new InvalidOperationException("Instructor account is disabled");
        }

        await EnsureRoleAssignedAsync(userAccount, "Instructor", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = userAccount.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new InstructorAuthResult(userAccount.Id, instructor.ToDto(), roles);
    }

    private async Task EnsureRoleAssignedAsync(UserAccount userAccount, string roleName, CancellationToken cancellationToken)
    {
        if (userAccount.UserRoles.Any(x => string.Equals(x.Role.Name, roleName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == roleName, cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                Name = roleName
            };
            _dbContext.Roles.Add(role);
        }

        userAccount.UserRoles.Add(new UserRole
        {
            User = userAccount,
            Role = role
        });
    }
}
