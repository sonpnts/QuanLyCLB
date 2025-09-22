using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IScheduleService
{
    Task<IReadOnlyCollection<ClassScheduleDto>> UpsertAsync(UpsertScheduleRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClassScheduleDto>> GetByClassAsync(Guid classId, CancellationToken cancellationToken = default);
}
