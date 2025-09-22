using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.Requests;

public record CreateStudentRequest(
    string FullName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Notes
);

public record UpdateStudentRequest(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Notes
);

public record ChangeStudentStatusRequest(
    Guid StudentId,
    StudentStatus Status
);
