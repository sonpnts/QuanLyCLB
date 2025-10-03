using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IInstructorService
{
    Task<PagedResult<InstructorDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<InstructorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InstructorDto> CreateAsync(CreateInstructorRequest request, CancellationToken cancellationToken = default);
    Task<InstructorDto?> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InstructorAuthResult> SyncGoogleAccountAsync(string email, string fullName, string googleSubject,string avatarUrl, CancellationToken cancellationToken = default);
}
