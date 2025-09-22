using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceSessionDto> CreateSessionAsync(CreateAttendanceSessionRequest request, Guid markedById, CancellationToken cancellationToken = default);
    Task<AttendanceSessionDto> MarkAsync(MarkAttendanceRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AttendanceSessionDto>> GetSessionsAsync(Guid classId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
