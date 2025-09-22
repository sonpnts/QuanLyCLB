using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.DTOs;

public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    Guid ClassId,
    DateTime EnrolledAt,
    EnrollmentStatus Status
);
