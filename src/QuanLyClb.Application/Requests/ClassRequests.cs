using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.Requests;

public record CreateClassRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    int Capacity,
    ClassStatus Status,
    Guid? CoachId,
    Guid? AssistantId
);

public record UpdateClassRequest(
    Guid Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    int Capacity,
    ClassStatus Status,
    Guid? CoachId,
    Guid? AssistantId
);

public record CloneClassRequest(
    Guid SourceClassId,
    string NewName,
    DateTime StartDate,
    DateTime? EndDate
);

public record ArchiveClassRequest(
    Guid ClassId,
    bool IsArchived
);
