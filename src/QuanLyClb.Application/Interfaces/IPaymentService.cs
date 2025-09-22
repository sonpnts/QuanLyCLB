using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IPaymentService
{
    Task<TuitionPaymentDto> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);
    Task<decimal> CloseoutAsync(CloseoutPaymentRequest request, CancellationToken cancellationToken = default);
}
