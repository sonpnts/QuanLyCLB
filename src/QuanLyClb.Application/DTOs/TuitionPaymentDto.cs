namespace QuanLyClb.Application.DTOs;

public record TuitionPaymentDto(
    Guid Id,
    Guid StudentId,
    Guid ClassId,
    decimal Amount,
    DateTime PaidAt,
    Guid? CollectedById,
    string? Notes
);
