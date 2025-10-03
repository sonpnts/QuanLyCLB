using System;
using System.Collections.Generic;
using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.DTOs;

public record ClassScheduleDto(
    Guid Id,
    Guid TrainingClassId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    BranchDto Branch
);

public record CreateClassScheduleRequest(
    Guid TrainingClassId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid BranchId
);

public record UpdateClassScheduleRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid BranchId
);

public record BulkCreateScheduleRequest(
    Guid TrainingClassId,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid BranchId
);
