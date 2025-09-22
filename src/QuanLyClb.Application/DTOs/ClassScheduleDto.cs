namespace QuanLyClb.Application.DTOs;

public record ClassScheduleDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string? Location
);
