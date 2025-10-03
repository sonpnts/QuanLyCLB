using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface ITrainingClassService
{
    Task<PagedResult<TrainingClassDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<TrainingClassDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TrainingClassDto> CreateAsync(CreateTrainingClassRequest request, CancellationToken cancellationToken = default);
    Task<TrainingClassDto?> UpdateAsync(Guid id, UpdateTrainingClassRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
