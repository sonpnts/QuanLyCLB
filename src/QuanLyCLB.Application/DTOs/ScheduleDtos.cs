using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.DTOs;

public record ClassScheduleDto(
    Guid Id,
    Guid TrainingClassId,
    DateOnly StudyDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DayOfWeek DayOfWeek,
    string LocationName,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters
);

public record CreateClassScheduleRequest(
    Guid TrainingClassId,
    DateOnly StudyDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DayOfWeek DayOfWeek,
    string LocationName,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters
);

public record UpdateClassScheduleRequest(
    DateOnly StudyDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DayOfWeek DayOfWeek,
    string LocationName,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters
);

public record BulkCreateScheduleRequest(
    Guid TrainingClassId,
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string LocationName,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters
);
