namespace QuanLyClb.Application.Requests;

public record TuitionReportRequest(
    DateTime From,
    DateTime To
);

public record ClassRosterRequest(
    Guid ClassId
);
