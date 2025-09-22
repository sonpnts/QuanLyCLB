using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.DTOs;

public record StudentDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    StudentStatus Status,
    Guid? CurrentClassId,
    string? Notes
);
