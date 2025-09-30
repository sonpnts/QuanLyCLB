using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("check-in")]
    [Authorize(Policy = "CoachOnly")]
    public async Task<ActionResult<AttendanceRecordDto>> CheckIn([FromBody] CheckInRequest request, CancellationToken cancellationToken)
    {
        if (!TryValidateCoach(request.CoachId))
        {
            return Forbid();
        }

        var record = await _attendanceService.CheckInAsync(request, cancellationToken);
        return Ok(record);
    }

    [HttpPost("manual")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AttendanceRecordDto>> CreateManual([FromBody] ManualAttendanceRequest request, CancellationToken cancellationToken)
    {
        var record = await _attendanceService.CreateManualAttendanceAsync(request, cancellationToken);
        return Ok(record);
    }

    [HttpGet("coach/{coachId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<AttendanceRecordDto>>> GetCoachAttendance(Guid coachId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !TryValidateCoach(coachId))
        {
            return Forbid();
        }

        var records = await _attendanceService.GetAttendanceByCoachAsync(coachId, fromDate, toDate, cancellationToken);
        return Ok(records);
    }

    [HttpPost("tickets")]
    [Authorize(Policy = "CoachOnly")]
    public async Task<ActionResult<AttendanceTicketDto>> CreateTicket([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        if (!TryValidateCoach(request.CoachId))
        {
            return Forbid();
        }

        var ticket = await _attendanceService.CreateTicketAsync(request, cancellationToken);
        return Ok(ticket);
    }

    [HttpPost("tickets/{ticketId:guid}/approval")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AttendanceTicketDto>> ApproveTicket(Guid ticketId, [FromBody] TicketApprovalRequest request, CancellationToken cancellationToken)
    {
        var ticket = await _attendanceService.ApproveTicketAsync(ticketId, request, cancellationToken);
        return ticket is not null ? Ok(ticket) : NotFound();
    }

    private bool TryValidateCoach(Guid coachId)
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var currentId) && currentId == coachId;
    }
}
