using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.DTOs;

public record AttendanceRecordDto(
    Guid Id,
    Guid ClassScheduleId,
    Guid InstructorId,
    DateTime CheckedInAt,
    double Latitude,
    double Longitude,
    AttendanceStatus Status,
    string Notes,
    Guid? TicketId
);

public record CheckInRequest(
    Guid ClassScheduleId,
    Guid InstructorId,
    DateTime CheckedInAt,
    double Latitude,
    double Longitude
);

public record ManualAttendanceRequest(
    Guid ClassScheduleId,
    Guid InstructorId,
    DateTime OccurredAt,
    AttendanceStatus Status,
    string Notes,
    Guid TicketId
);

public record AttendanceTicketDto(
    Guid Id,
    Guid ClassScheduleId,
    Guid InstructorId,
    DateTime CreatedAt,
    string Reason,
    string CreatedBy,
    bool IsApproved,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    Guid? CreatedByUserId = null,
    DateTime? UpdatedAt = null,
    Guid? UpdatedByUserId = null
);

public record CreateTicketRequest(
    Guid ClassScheduleId,
    Guid InstructorId,
    string Reason,
    string CreatedBy,
    Guid? CreatedByUserId = null
);

public record TicketApprovalRequest(
    bool Approve,
    string Approver,
    string? Notes,
    Guid? UpdatedByUserId = null
);
