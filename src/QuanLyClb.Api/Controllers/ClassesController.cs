using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Api.Authorization;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission("Classes", PermissionAction.View)]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly IScheduleService _scheduleService;

    public ClassesController(IClassService classService, IScheduleService scheduleService)
    {
        _classService = classService;
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var classes = await _classService.SearchAsync(keyword, cancellationToken);
        return Ok(classes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var trainingClass = await _classService.GetByIdAsync(id, cancellationToken);
        if (trainingClass is null)
        {
            return NotFound();
        }

        return Ok(trainingClass);
    }

    [HttpPost]
    [HasPermission("Classes", PermissionAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var trainingClass = await _classService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = trainingClass.Id }, trainingClass);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Classes", PermissionAction.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("Id mismatch");
        }

        var trainingClass = await _classService.UpdateAsync(request, cancellationToken);
        return Ok(trainingClass);
    }

    [HttpPost("clone")]
    [HasPermission("Classes", PermissionAction.Create)]
    public async Task<IActionResult> Clone([FromBody] CloneClassRequest request, CancellationToken cancellationToken)
    {
        var trainingClass = await _classService.CloneAsync(request, cancellationToken);
        return Ok(trainingClass);
    }

    [HttpPost("{id:guid}/archive")]
    [HasPermission("Classes", PermissionAction.Update)]
    public async Task<IActionResult> Archive(Guid id, [FromBody] ArchiveClassRequest request, CancellationToken cancellationToken)
    {
        if (id != request.ClassId)
        {
            return BadRequest("Id mismatch");
        }

        await _classService.ArchiveAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/schedule")]
    [HasPermission("Schedules", PermissionAction.Update)]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpsertScheduleRequest request, CancellationToken cancellationToken)
    {
        if (id != request.ClassId)
        {
            return BadRequest("Id mismatch");
        }

        var schedules = await _scheduleService.UpsertAsync(request, cancellationToken);
        return Ok(schedules);
    }

    [HttpGet("{id:guid}/schedule")]
    [HasPermission("Schedules", PermissionAction.View)]
    public async Task<IActionResult> GetSchedule(Guid id, CancellationToken cancellationToken)
    {
        var schedules = await _scheduleService.GetByClassAsync(id, cancellationToken);
        return Ok(schedules);
    }
}
