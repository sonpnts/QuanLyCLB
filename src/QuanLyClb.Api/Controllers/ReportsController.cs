using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("tuition")]
    public async Task<IActionResult> TuitionReport([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var request = new TuitionReportRequest(from, to);
        var report = await _reportService.GetTuitionReportAsync(request, cancellationToken);
        return Ok(report);
    }

    [HttpGet("classes/{classId:guid}/roster")]
    public async Task<IActionResult> ClassRoster(Guid classId, CancellationToken cancellationToken)
    {
        var roster = await _reportService.GetClassRosterAsync(new ClassRosterRequest(classId), cancellationToken);
        if (roster is null)
        {
            return NotFound();
        }

        return Ok(roster);
    }
}
