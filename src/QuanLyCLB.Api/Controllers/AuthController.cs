using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Infrastructure.Settings;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IGoogleTokenValidator _tokenValidator;
    private readonly IInstructorService _instructorService;
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILoginAuditService _loginAuditService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IGoogleTokenValidator tokenValidator,
        IInstructorService instructorService,
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        ILoginAuditService loginAuditService,
        IOptions<JwtSettings> jwtOptions)
    {
        _tokenValidator = tokenValidator;
        _instructorService = instructorService;
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _loginAuditService = loginAuditService;
        _jwtSettings = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> LoginWithGoogle([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var apiEndpoint = HttpContext.Request.Path;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        const string provider = "Google";

        var googleUser = await _tokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (googleUser is null)
        {
            await _loginAuditService.RecordAsync(
                null,
                request.IdToken,
                provider,
                false,
                apiEndpoint,
                request.LocationAddress,
                request.DeviceInfo,
                ipAddress,
                "Invalid Google token",
                cancellationToken);
            return Unauthorized();
        }

        InstructorAuthResult instructorResult;
        try
        {
            instructorResult = await _instructorService.SyncGoogleAccountAsync(googleUser.Email, googleUser.Name, googleUser.Subject, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await _loginAuditService.RecordAsync(
                null,
                googleUser.Email,
                provider,
                false,
                apiEndpoint,
                request.LocationAddress,
                request.DeviceInfo,
                ipAddress,
                ex.Message,
                cancellationToken);
            return Unauthorized(new { message = ex.Message });
        }
        var instructor = instructorResult.Instructor;
        var roles = instructorResult.Roles;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, instructor.Id.ToString()),
            new(ClaimTypes.Email, instructor.Email),
            new(ClaimTypes.Name, instructor.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.ExpirationDays);
        var token = _jwtTokenService.CreateToken(claims, expiresAt);

        await _loginAuditService.RecordAsync(
            instructorResult.UserAccountId,
            googleUser.Email,
            provider,
            true,
            apiEndpoint,
            request.LocationAddress,
            request.DeviceInfo,
            ipAddress,
            null,
            cancellationToken);

        return Ok(new AuthResponse(token, expiresAt, instructor, roles));
    }

    [AllowAnonymous]
    [HttpPost("password")]
    public async Task<ActionResult<AuthResponse>> LoginWithPassword([FromBody] PasswordLoginRequest request, CancellationToken cancellationToken)
    {
        var apiEndpoint = HttpContext.Request.Path;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        const string provider = "Password";

        InstructorAuthResult instructorResult;
        try
        {
            instructorResult = await _authService.AuthenticateWithCredentialsAsync(request.Username, request.Password, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await _loginAuditService.RecordAsync(
                null,
                request.Username,
                provider,
                false,
                apiEndpoint,
                request.LocationAddress,
                request.DeviceInfo,
                ipAddress,
                ex.Message,
                cancellationToken);
            return Unauthorized(new { message = ex.Message });
        }

        var instructor = instructorResult.Instructor;
        var roles = instructorResult.Roles;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, instructor.Id.ToString()),
            new(ClaimTypes.Email, instructor.Email),
            new(ClaimTypes.Name, instructor.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.ExpirationDays);
        var token = _jwtTokenService.CreateToken(claims, expiresAt);

        await _loginAuditService.RecordAsync(
            instructorResult.UserAccountId,
            request.Username,
            provider,
            true,
            apiEndpoint,
            request.LocationAddress,
            request.DeviceInfo,
            ipAddress,
            null,
            cancellationToken);

        return Ok(new AuthResponse(token, expiresAt, instructor, roles));
    }
}
