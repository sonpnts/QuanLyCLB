namespace QuanLyCLB.Application.DTOs;

public record GoogleLoginRequest(
    string IdToken
  );

public record PasswordLoginRequest(
    string Username,
    string Password
    );

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, InstructorDto Instructor, IReadOnlyCollection<string> Roles);
