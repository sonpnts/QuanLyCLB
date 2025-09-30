using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Application.Mappings;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class UserService : IUserService
{
    private const int OtpLength = 6;
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);

    private readonly ClubManagementDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public UserService(ClubManagementDbContext dbContext, IAuthService authService, IEmailService emailService)
    {
        _dbContext = dbContext;
        _authService = authService;
        _emailService = emailService;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return users.Select(x => x.ToDto()).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return user?.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username is required.", nameof(request.Username));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(request.FullName));
        }

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim();
        var normalizedFullName = request.FullName.Trim();
        var normalizedPhone = request.PhoneNumber?.Trim() ?? string.Empty;

        if (await _dbContext.Users.AnyAsync(x => x.Username == normalizedUsername, cancellationToken))
        {
            throw new InvalidOperationException($"Username '{normalizedUsername}' is already taken");
        }

        if (await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException($"Email '{normalizedEmail}' is already registered");
        }

        var userAccount = new UserAccount
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            FullName = normalizedFullName,
            PhoneNumber = normalizedPhone,
            AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim(),
            IsActive = request.IsActive
        };

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var (hash, salt) = _authService.HashPassword(request.Password);
            userAccount.PasswordHash = hash;
            userAccount.PasswordSalt = salt;
        }

        _dbContext.Users.Add(userAccount);

        await SyncRolesAsync(userAccount, request.Roles, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Entry(userAccount)
            .Collection(x => x.UserRoles)
            .Query()
            .Include(x => x.Role)
            .LoadAsync(cancellationToken);

        return userAccount.ToDto();
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(request.FullName));
        }

        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Instructor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (userAccount is null)
        {
            return null;
        }

        var normalizedFullName = request.FullName.Trim();
        var normalizedPhone = request.PhoneNumber?.Trim() ?? string.Empty;

        userAccount.FullName = normalizedFullName;
        userAccount.PhoneNumber = normalizedPhone;
        userAccount.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        userAccount.IsActive = request.IsActive;
        userAccount.UpdatedAt = DateTime.UtcNow;

        if (userAccount.Instructor is not null)
        {
            userAccount.Instructor.IsActive = request.IsActive;
            userAccount.Instructor.UpdatedAt = DateTime.UtcNow;
        }

        await SyncRolesAsync(userAccount, request.Roles, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userAccount.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userAccount = await _dbContext.Users
            .Include(x => x.Instructor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (userAccount is null)
        {
            return false;
        }

        if (userAccount.Instructor is not null)
        {
            throw new InvalidOperationException("Cannot delete user that is linked to an instructor");
        }

        _dbContext.Users.Remove(userAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserDto?> ChangeEmailAsync(Guid id, ChangeUserEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.NewEmail))
        {
            throw new ArgumentException("New email is required.", nameof(request.NewEmail));
        }

        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (userAccount is null)
        {
            return null;
        }

        var normalizedEmail = request.NewEmail.Trim();

        if (!string.Equals(userAccount.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _dbContext.Users
                .AnyAsync(x => x.Id != id && x.Email == normalizedEmail, cancellationToken);
            if (emailExists)
            {
                throw new InvalidOperationException($"Email '{normalizedEmail}' is already registered");
            }

            userAccount.Email = normalizedEmail;
        }

        if (request.UpdateUsername)
        {
            if (!string.Equals(userAccount.Username, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                var usernameExists = await _dbContext.Users
                    .AnyAsync(x => x.Id != id && x.Username == normalizedEmail, cancellationToken);
                if (usernameExists)
                {
                    throw new InvalidOperationException($"Username '{normalizedEmail}' is already taken");
                }

                userAccount.Username = normalizedEmail;
            }
        }

        userAccount.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userAccount.ToDto();
    }

    public async Task<UserDto?> UpdateFullNameAsync(Guid id, UpdateUserNameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(request.FullName));
        }

        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (userAccount is null)
        {
            return null;
        }

        userAccount.FullName = request.FullName.Trim();
        userAccount.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return userAccount.ToDto();
    }

    public async Task SendPasswordResetOtpAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        var normalizedEmail = request.Email.Trim();
        var userAccount = await _dbContext.Users
            .Include(x => x.PasswordResetOtps)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (userAccount is null || !userAccount.IsActive)
        {
            throw new InvalidOperationException("Email is not registered or account is inactive");
        }

        if (string.IsNullOrWhiteSpace(userAccount.PasswordHash) || string.IsNullOrWhiteSpace(userAccount.PasswordSalt))
        {
            throw new InvalidOperationException("User does not have a local password configured");
        }

        var now = DateTime.UtcNow;
        foreach (var otp in userAccount.PasswordResetOtps.Where(x => !x.IsUsed))
        {
            otp.IsUsed = true;
        }

        var otpCode = GenerateOtpCode();
        var otpEntity = new PasswordResetOtp
        {
            UserAccountId = userAccount.Id,
            CodeHash = HashOtp(otpCode),
            ExpiresAt = now.Add(OtpLifetime),
            IsUsed = false
        };

        userAccount.PasswordResetOtps.Add(otpEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var subject = "OTP khôi phục mật khẩu";
        var bodyBuilder = new StringBuilder();
        bodyBuilder.AppendLine($"<p>Xin chào {userAccount.FullName},</p>");
        bodyBuilder.AppendLine("<p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản QuanLyCLB.</p>");
        bodyBuilder.AppendLine($"<p>Mã OTP của bạn là: <strong>{otpCode}</strong></p>");
        bodyBuilder.AppendLine("<p>Mã sẽ hết hạn sau 10 phút. Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>");
        bodyBuilder.AppendLine("<p>Trân trọng,<br/>QuanLyCLB Team</p>");

        await _emailService.SendEmailAsync(userAccount.Email, subject, bodyBuilder.ToString(), cancellationToken: cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.OtpCode))
        {
            throw new ArgumentException("OTP code is required.", nameof(request.OtpCode));
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("New password must not be empty", nameof(request.NewPassword));
        }

        var normalizedEmail = request.Email.Trim();
        var userAccount = await _dbContext.Users
            .Include(x => x.PasswordResetOtps)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (userAccount is null || !userAccount.IsActive)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var otpHash = HashOtp(request.OtpCode);
        var otp = userAccount.PasswordResetOtps
            .Where(x => !x.IsUsed && x.ExpiresAt >= now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => string.Equals(x.CodeHash, otpHash, StringComparison.Ordinal));

        if (otp is null)
        {
            return false;
        }

        otp.IsUsed = true;
        otp.VerifiedAt = now;

        var (hash, salt) = _authService.HashPassword(request.NewPassword);
        userAccount.PasswordHash = hash;
        userAccount.PasswordSalt = salt;
        userAccount.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SyncRolesAsync(UserAccount userAccount, IReadOnlyCollection<string>? roles, CancellationToken cancellationToken)
    {
        if (roles is null)
        {
            return;
        }

        var normalizedRoles = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rolesToRemove = userAccount.UserRoles
            .Where(ur => normalizedRoles.All(role => !string.Equals(role, ur.Role.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (rolesToRemove.Count > 0)
        {
            _dbContext.UserRoles.RemoveRange(rolesToRemove);
        }

        var currentRoles = userAccount.UserRoles
            .Select(ur => ur.Role.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in normalizedRoles)
        {
            if (currentRoles.Contains(roleName))
            {
                continue;
            }

            var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == roleName, cancellationToken);
            if (role is null)
            {
                role = new Role { Name = roleName };
                _dbContext.Roles.Add(role);
            }

            userAccount.UserRoles.Add(new UserRole
            {
                User = userAccount,
                Role = role
            });
        }
    }

    private static string GenerateOtpCode()
    {
        var upperBound = (int)Math.Pow(10, OtpLength);
        var otp = RandomNumberGenerator.GetInt32(0, upperBound);
        return otp.ToString($"D{OtpLength}");
    }

    private static string HashOtp(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
