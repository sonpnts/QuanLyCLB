using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Entities;
using QuanLyClb.Domain.Enums;
using QuanLyClb.Infrastructure.Auth;
using QuanLyClb.Infrastructure.Configurations;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IOptions<GoogleAuthOptions> _googleOptions;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(ApplicationDbContext dbContext, IJwtTokenGenerator tokenGenerator, IOptions<GoogleAuthOptions> googleOptions)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
        _googleOptions = googleOptions;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Email already registered");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = normalizedEmail,
            Role = request.Role
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResultDto(user.Id, user.FullName, user.Email, user.Role, token, expiresAt);
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Account does not allow password login");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResultDto(user.Id, user.FullName, user.Email, user.Role, token, expiresAt);
    }

    public async Task<AuthResultDto> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings();
        if (!string.IsNullOrWhiteSpace(_googleOptions.Value.ClientId))
        {
            settings.Audience = new[] { _googleOptions.Value.ClientId };
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google token", ex);
        }

        var normalizedEmail = payload.Email.ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Email = normalizedEmail,
                FullName = payload.Name ?? payload.Email,
                GoogleId = payload.Subject,
                Role = UserRole.Coach
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = payload.Subject;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResultDto(user.Id, user.FullName, user.Email, user.Role, token, expiresAt);
    }
}
