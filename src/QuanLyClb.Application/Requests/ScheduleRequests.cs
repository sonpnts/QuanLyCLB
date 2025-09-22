namespace QuanLyClb.Application.Requests;

public record UpsertScheduleRequest(
    Guid ClassId,
    IReadOnlyCollection<ScheduleItem> Items
);

public record ScheduleItem(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string? Location
);
