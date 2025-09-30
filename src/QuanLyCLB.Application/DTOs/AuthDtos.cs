namespace QuanLyCLB.Application.DTOs;

public record GoogleLoginRequest(
    string IdToken,
    string? LocationAddress = null,
    string? DeviceInfo = null);

public record PasswordLoginRequest(
    string Username,
    string Password,
    string? LocationAddress = null,
    string? DeviceInfo = null);

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, InstructorDto Instructor, IReadOnlyCollection<string> Roles);
