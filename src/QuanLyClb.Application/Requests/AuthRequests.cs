using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.Requests;

public record RegisterUserRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role
);

public record LoginRequest(
    string Email,
    string Password
);

public record GoogleLoginRequest(
    string IdToken
);
