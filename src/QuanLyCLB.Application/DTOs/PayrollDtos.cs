namespace QuanLyCLB.Application.DTOs;

public record PayrollDetailDto(
    Guid AttendanceRecordId,
    DateTime CheckedInAt,
    decimal Hours,
    decimal Amount
);

public record PayrollPeriodDto(
    Guid Id,
    Guid InstructorId,
    int Year,
    int Month,
    decimal TotalHours,
    decimal TotalAmount,
    DateTime GeneratedAt,
    IReadOnlyCollection<PayrollDetailDto> Details
);

public record GeneratePayrollRequest(
    Guid InstructorId,
    int Year,
    int Month
);
