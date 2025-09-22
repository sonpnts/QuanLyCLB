using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(EnrollStudentRequest request, CancellationToken cancellationToken = default);
    Task TransferAsync(TransferStudentRequest request, CancellationToken cancellationToken = default);
}
