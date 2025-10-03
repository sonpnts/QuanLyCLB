using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IScheduleService
{
    Task<IReadOnlyCollection<ClassScheduleDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClassScheduleDto>> BulkCreateAsync(BulkCreateScheduleRequest request, CancellationToken cancellationToken = default);
    Task<ClassScheduleDto?> UpdateAsync(Guid id, UpdateClassScheduleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
