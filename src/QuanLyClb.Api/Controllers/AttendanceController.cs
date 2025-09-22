using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Api.Authorization;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/classes/{classId:guid}/[controller]")]
[HasPermission("Attendance", PermissionAction.View)]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(Guid classId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var sessions = await _attendanceService.GetSessionsAsync(classId, from, to, cancellationToken);
        return Ok(sessions);
    }

    [HttpPost]
    [HasPermission("Attendance", PermissionAction.Create)]
    public async Task<IActionResult> CreateSession(Guid classId, [FromBody] CreateAttendanceSessionRequest request, CancellationToken cancellationToken)
    {
        if (classId != request.ClassId)
        {
            return BadRequest("Id mismatch");
        }

        var markedById = GetUserId();
        var session = await _attendanceService.CreateSessionAsync(request, markedById, cancellationToken);
        return Ok(session);
    }

    [HttpPost("{sessionId:guid}/records")]
    [HasPermission("Attendance", PermissionAction.Update)]
    public async Task<IActionResult> MarkAttendance(Guid classId, Guid sessionId, [FromBody] MarkAttendanceRequest request, CancellationToken cancellationToken)
    {
        if (sessionId != request.SessionId)
        {
            return BadRequest("Id mismatch");
        }

        var session = await _attendanceService.MarkAsync(request, cancellationToken);
        return Ok(session);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed) ? parsed : Guid.Empty;
    }
}
