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

        var query = _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Where(x => x.UserRoles.Any(r => r.Role.Name == RoleNames.Coach))
            .OrderBy(x => x.FullName);

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = users
            .Select(x => x.ToInstructorDto())
            .ToList();

        return new PagedResult<InstructorDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<InstructorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return user.ToInstructorDto();
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim();

        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (userAccount is null)
        {
            userAccount = new UserAccount
            {
                Username = normalizedEmail,
                Email = normalizedEmail,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                SkillLevel = request.SkillLevel,
                Certification = request.Certification,
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

            userAccount.FullName = request.FullName;
            userAccount.PhoneNumber = request.PhoneNumber;
            userAccount.SkillLevel = request.SkillLevel;
            userAccount.Certification = request.Certification;
        }

        await EnsureRoleAssignedAsync(userAccount, RoleNames.Coach, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userAccount.ToInstructorDto();
    }

    public async Task<InstructorDto?> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (userAccount is null)
        {
            return null;
        }

        userAccount.FullName = request.FullName;
        userAccount.PhoneNumber = request.PhoneNumber;
        userAccount.SkillLevel = request.SkillLevel;
        userAccount.Certification = request.Certification;
        userAccount.IsActive = request.IsActive;
        userAccount.UpdatedAt = DateTime.UtcNow;

        if (!request.IsActive)
        {
            RemoveRoleAssignment(userAccount, RoleNames.Coach);
        }
        else
        {
            await EnsureRoleAssignedAsync(userAccount, RoleNames.Coach, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return userAccount.ToInstructorDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (userAccount is null)
        {
            return false;
        }

        RemoveRoleAssignment(userAccount, RoleNames.Coach);
        userAccount.IsActive = false;
        userAccount.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<InstructorAuthResult> SyncGoogleAccountAsync(string email, string fullName, string googleSubject,string avatarUrl, CancellationToken cancellationToken = default)
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
            userAccount.AvatarUrl = avatarUrl;
        }
        else if (!string.Equals(userAccount.GoogleSubject, googleSubject, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Google account does not match the registered email");
        }

        // userAccount.FullName = fullName;
        await EnsureRoleAssignedAsync(userAccount, RoleNames.Coach, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = userAccount.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new InstructorAuthResult(userAccount.Id, userAccount.ToInstructorDto(), roles);
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

    private static void RemoveRoleAssignment(UserAccount userAccount, string roleName)
    {
        var roleAssignment = userAccount.UserRoles
            .FirstOrDefault(x => string.Equals(x.Role.Name, roleName, StringComparison.OrdinalIgnoreCase));

        if (roleAssignment is not null)
        {
            userAccount.UserRoles.Remove(roleAssignment);
        }
    }

    private static class RoleNames
    {
        public const string Coach = "Coach";
    }
}
