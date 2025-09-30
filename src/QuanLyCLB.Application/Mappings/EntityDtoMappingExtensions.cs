using System;
using System.Linq;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Application.Mappings;

public static class EntityDtoMappingExtensions
{
    public static UserDto ToDto(this UserAccount entity) => new(
        entity.Id,
        entity.Username,
        entity.Email,
        entity.FullName,
        entity.PhoneNumber,
        entity.AvatarUrl,
        entity.IsActive,
        !string.IsNullOrWhiteSpace(entity.PasswordHash),
        !string.IsNullOrWhiteSpace(entity.GoogleSubject),
        entity.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList());

    public static InstructorDto ToInstructorDto(this UserAccount entity) => new(
        entity.Id,
        entity.FullName,
        entity.Email,
        entity.PhoneNumber,
        entity.SkillLevel,
        entity.Certification,
        entity.IsActive);

    public static TrainingClassDto ToDto(this TrainingClass entity) => new(
        entity.Id,
        entity.Code,
        entity.Name,
        entity.Description,
        entity.StartDate,
        entity.EndDate,
        entity.MaxStudents,
        entity.CoachId);

    public static BranchDto ToDto(this Branch entity) => new(
        entity.Id,
        entity.Name,
        entity.Address,
        entity.Latitude,
        entity.Longitude,
        entity.AllowedRadiusMeters,
        //entity.GooglePlaceId,
        entity.GoogleMapsEmbedUrl);

    public static ClassScheduleDto ToDto(this ClassSchedule entity) => new(
        entity.Id,
        entity.TrainingClassId,
        entity.DayOfWeek,
        entity.StartTime,
        entity.EndTime,
        entity.Branch?.ToDto() ?? throw new InvalidOperationException("Class schedule is missing branch information"));

    public static AttendanceRecordDto ToDto(this AttendanceRecord entity) => new(
        entity.Id,
        entity.ClassScheduleId,
        entity.CoachId,
        entity.CheckedInAt,
        entity.Latitude,
        entity.Longitude,
        entity.Status,
        entity.Notes,
        entity.TicketId);

    public static AttendanceTicketDto ToDto(this AttendanceTicket entity) => new(
        entity.Id,
        entity.ClassScheduleId,
        entity.CoachId,
        entity.CreatedAt,
        entity.Reason,
        entity.CreatedBy,
        entity.IsApproved,
        entity.ApprovedBy,
        entity.ApprovedAt,
        entity.CreatedByUserId,
        entity.UpdatedAt,
        entity.UpdatedByUserId);

    public static PayrollPeriodDto ToDto(this PayrollPeriod entity) => new(
        entity.Id,
        entity.CoachId,
        entity.Year,
        entity.Month,
        entity.TotalHours,
        entity.TotalAmount,
        entity.GeneratedAt,
        entity.Details
            .Select(detail => new PayrollDetailDto(
                detail.AttendanceRecordId,
                detail.AttendanceRecord?.CheckedInAt ?? DateTime.MinValue,
                detail.Hours,
                detail.Amount))
            .ToList());
}
