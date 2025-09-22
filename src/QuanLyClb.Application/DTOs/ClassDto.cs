using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Application.DTOs;

public record ClassDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    int Capacity,
    ClassStatus Status,
    bool IsArchived,
    Guid? CoachId,
    Guid? AssistantId,
    Guid? ParentClassId
);
