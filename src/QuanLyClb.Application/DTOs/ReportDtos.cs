namespace QuanLyClb.Application.DTOs;

public record TuitionReportItemDto(
    Guid ClassId,
    string ClassName,
    decimal TotalCollected,
    int PaymentCount
);

public record ClassRosterItemDto(
    Guid StudentId,
    string FullName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth
);

public record ClassRosterDto(
    Guid ClassId,
    string ClassName,
    IReadOnlyCollection<ClassRosterItemDto> Students
);
