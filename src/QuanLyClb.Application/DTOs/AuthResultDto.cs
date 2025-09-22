using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.DTOs;

public record AuthResultDto(
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role,
    string Token,
    DateTime ExpiresAt
);
