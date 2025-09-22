using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IClassService
{
    Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<ClassDto> UpdateAsync(UpdateClassRequest request, CancellationToken cancellationToken = default);
    Task<ClassDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClassDto>> SearchAsync(string? keyword, CancellationToken cancellationToken = default);
    Task<ClassDto> CloneAsync(CloneClassRequest request, CancellationToken cancellationToken = default);
    Task ArchiveAsync(ArchiveClassRequest request, CancellationToken cancellationToken = default);
}
