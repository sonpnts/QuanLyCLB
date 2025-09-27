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
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IGoogleTokenValidator tokenValidator,
        IInstructorService instructorService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _tokenValidator = tokenValidator;
        _instructorService = instructorService;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> LoginWithGoogle([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var googleUser = await _tokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (googleUser is null)
        {
            return Unauthorized();
        }

        InstructorAuthResult instructorResult;
        try
        {
            instructorResult = await _instructorService.SyncGoogleAccountAsync(googleUser.Email, googleUser.Name, googleUser.Subject, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
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

        return Ok(new AuthResponse(token, expiresAt, instructor, roles));
    }
}
