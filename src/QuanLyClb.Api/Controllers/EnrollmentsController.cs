using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Api.Authorization;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [HasPermission("Enrollments", PermissionAction.Create)]
    public async Task<IActionResult> Enroll([FromBody] EnrollStudentRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentService.EnrollAsync(request, cancellationToken);
        return Ok(enrollment);
    }

    [HttpPost("transfer")]
    [HasPermission("Enrollments", PermissionAction.Update)]
    public async Task<IActionResult> Transfer([FromBody] TransferStudentRequest request, CancellationToken cancellationToken)
    {
        await _enrollmentService.TransferAsync(request, cancellationToken);
        return NoContent();
    }
}
