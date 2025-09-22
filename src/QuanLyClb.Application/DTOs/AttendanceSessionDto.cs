using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.DTOs;

public record AttendanceRecordDto(
    Guid Id,
    Guid StudentId,
    AttendanceStatus Status,
    DateTime MarkedAt
);

public record AttendanceSessionDto(
    Guid Id,
    Guid ClassId,
    DateTime SessionDate,
    Guid? CoachId,
    Guid? MarkedById,
    string? PhotoUrl,
    string? Notes,
    IReadOnlyCollection<AttendanceRecordDto> Records
);
