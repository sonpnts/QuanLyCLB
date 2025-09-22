using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.Requests;

public record CreateAttendanceSessionRequest(
    Guid ClassId,
    DateTime SessionDate,
    Guid? CoachId,
    string? PhotoUrl,
    string? Notes
);

public record MarkAttendanceRequest(
    Guid SessionId,
    Guid StudentId,
    AttendanceStatus Status
);
