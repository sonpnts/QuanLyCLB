using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollStudentRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentService.EnrollAsync(request, cancellationToken);
        return Ok(enrollment);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferStudentRequest request, CancellationToken cancellationToken)
    {
        await _enrollmentService.TransferAsync(request, cancellationToken);
        return NoContent();
    }
}
