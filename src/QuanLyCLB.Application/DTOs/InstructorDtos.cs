namespace QuanLyCLB.Application.DTOs;

public record InstructorDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    decimal HourlyRate,
    bool IsActive
);

public record CreateInstructorRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    decimal HourlyRate
);

public record UpdateInstructorRequest(
    string FullName,
    string PhoneNumber,
    decimal HourlyRate,
    bool IsActive
);

public record InstructorAuthResult(
    Guid UserAccountId,
    InstructorDto Instructor,
    IReadOnlyCollection<string> Roles
);
