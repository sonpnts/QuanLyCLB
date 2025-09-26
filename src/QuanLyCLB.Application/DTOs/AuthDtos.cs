namespace QuanLyCLB.Application.DTOs;

public record GoogleLoginRequest(string IdToken);

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, InstructorDto Instructor);
