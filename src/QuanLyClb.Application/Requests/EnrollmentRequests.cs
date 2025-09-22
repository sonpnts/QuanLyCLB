namespace QuanLyClb.Application.Requests;

public record EnrollStudentRequest(
    Guid StudentId,
    Guid ClassId
);

public record TransferStudentRequest(
    Guid StudentId,
    Guid FromClassId,
    Guid ToClassId
);
