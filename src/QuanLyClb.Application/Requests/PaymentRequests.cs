namespace QuanLyClb.Application.Requests;

public record RecordPaymentRequest(
    Guid StudentId,
    Guid ClassId,
    decimal Amount,
    Guid? CollectedById,
    string? Notes
);

public record CloseoutPaymentRequest(
    Guid CollectorId,
    DateTime From,
    DateTime To
);
