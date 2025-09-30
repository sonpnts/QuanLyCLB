using System;
using System.Collections.Generic;

namespace QuanLyCLB.Application.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string PhoneNumber,
    string? AvatarUrl,
    string SkillLevel,
    string? Certification,
    bool IsActive,
    bool HasPassword,
    bool IsGoogleAccount,
    IReadOnlyCollection<string> Roles);

public record CreateUserRequest(
    string Username,
    string Email,
    string FullName,
    string PhoneNumber,
    string? Password,
    bool IsActive,
    string? SkillLevel,
    string? Certification,
    string? AvatarUrl = null,
    IReadOnlyCollection<string>? Roles = null);

public record UpdateUserRequest(
    string FullName,
    string PhoneNumber,
    string? AvatarUrl,
    bool IsActive,
    string? SkillLevel,
    string? Certification,
    IReadOnlyCollection<string>? Roles = null);

public record ChangeUserEmailRequest(string NewEmail, bool UpdateUsername = true);

public record UpdateUserNameRequest(string FullName);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string OtpCode, string NewPassword);
