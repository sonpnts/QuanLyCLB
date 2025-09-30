using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceRecordDto> CheckInAsync(CheckInRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceRecordDto> CreateManualAttendanceAsync(ManualAttendanceRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AttendanceRecordDto>> GetAttendanceByCoachAsync(Guid coachId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<AttendanceTicketDto> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceTicketDto?> ApproveTicketAsync(Guid ticketId, TicketApprovalRequest request, CancellationToken cancellationToken = default);
}
