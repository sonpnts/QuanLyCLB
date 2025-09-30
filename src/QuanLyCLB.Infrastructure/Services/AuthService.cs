using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Application.Mappings;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const int SaltSize = 16; // 128-bit salt
    private const int KeySize = 32; // 256-bit key
    private const int Iterations = 100_000;

    private readonly ClubManagementDbContext _dbContext;

    public AuthService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InstructorAuthResult> AuthenticateWithCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();

        var userAccount = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == normalizedUsername || x.Email == normalizedUsername, cancellationToken);

        if (userAccount is null)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        if (!userAccount.IsActive)
        {
            throw new InvalidOperationException("User account is disabled.");
        }

        if (string.IsNullOrWhiteSpace(userAccount.PasswordHash) || string.IsNullOrWhiteSpace(userAccount.PasswordSalt))
        {
            throw new InvalidOperationException("User account does not have a password configured.");
        }

        if (!VerifyPassword(password, userAccount.PasswordHash, userAccount.PasswordSalt))
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        if (!userAccount.UserRoles.Any(r => string.Equals(r.Role.Name, "Coach", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Coach is not registered in the system.");
        }

        var roles = userAccount.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new InstructorAuthResult(userAccount.Id, userAccount.ToInstructorDto(), roles);
    }

    public (string Hash, string Salt) HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = PBKDF2(password, saltBytes, Iterations, KeySize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var saltBytes = Convert.FromBase64String(storedSalt);
        var hashBytes = PBKDF2(password, saltBytes, Iterations, KeySize);
        var storedHashBytes = Convert.FromBase64String(storedHash);

        return CryptographicOperations.FixedTimeEquals(hashBytes, storedHashBytes);
    }

    private static byte[] PBKDF2(string password, byte[] salt, int iterations, int keySize)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, HashAlgorithmName.SHA256, keySize);
    }
}
