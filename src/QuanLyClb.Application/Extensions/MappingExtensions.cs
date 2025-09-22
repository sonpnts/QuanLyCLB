using QuanLyClb.Application.DTOs;
using QuanLyClb.Domain.Entities;

namespace QuanLyClb.Application.Extensions;

public static class MappingExtensions
{
    public static StudentDto ToDto(this Student entity) => new(
        entity.Id,
        entity.FullName,
        entity.Email,
        entity.PhoneNumber,
        entity.DateOfBirth,
        entity.Status,
        entity.CurrentClassId,
        entity.Notes
    );

    public static ClassDto ToDto(this TrainingClass entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.StartDate,
        entity.EndDate,
        entity.Capacity,
        entity.Status,
        entity.IsArchived,
        entity.CoachId,
        entity.AssistantId,
        entity.ParentClassId
    );

    public static EnrollmentDto ToDto(this Enrollment entity) => new(
        entity.Id,
        entity.StudentId,
        entity.ClassId,
        entity.EnrolledAt,
        entity.Status
    );

    public static ClassScheduleDto ToDto(this ClassSchedule entity) => new(
        entity.Id,
        entity.DayOfWeek,
        entity.StartTime,
        entity.EndTime,
        entity.Location
    );

    public static AttendanceRecordDto ToDto(this AttendanceRecord entity) => new(
        entity.Id,
        entity.StudentId,
        entity.Status,
        entity.MarkedAt
    );

    public static AttendanceSessionDto ToDto(this AttendanceSession entity) => new(
        entity.Id,
        entity.ClassId,
        entity.SessionDate,
        entity.CoachId,
        entity.MarkedById,
        entity.PhotoUrl,
        entity.Notes,
        entity.Records.Select(r => r.ToDto()).ToList()
    );

    public static TuitionPaymentDto ToDto(this TuitionPayment entity) => new(
        entity.Id,
        entity.StudentId,
        entity.ClassId,
        entity.Amount,
        entity.PaidAt,
        entity.CollectedById,
        entity.Notes
    );
}
