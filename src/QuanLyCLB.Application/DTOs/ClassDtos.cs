namespace QuanLyCLB.Application.DTOs;

public record TrainingClassDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    int MaxStudents,
    Guid InstructorId
);

public record CreateTrainingClassRequest(
    string Code,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    int MaxStudents,
    Guid InstructorId
);

public record UpdateTrainingClassRequest(
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    int MaxStudents,
    Guid InstructorId
);
