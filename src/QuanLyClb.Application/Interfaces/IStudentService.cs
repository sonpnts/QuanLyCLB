using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IStudentService
{
    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentDto> UpdateAsync(UpdateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<StudentDto>> SearchAsync(string? keyword, CancellationToken cancellationToken = default);
    Task ChangeStatusAsync(ChangeStudentStatusRequest request, CancellationToken cancellationToken = default);
}
