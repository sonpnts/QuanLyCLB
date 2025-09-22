using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyCollection<TuitionReportItemDto>> GetTuitionReportAsync(TuitionReportRequest request, CancellationToken cancellationToken = default);
    Task<ClassRosterDto?> GetClassRosterAsync(ClassRosterRequest request, CancellationToken cancellationToken = default);
}
