using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassScheduleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleService.GetByIdAsync(id, cancellationToken);
        return schedule is not null ? Ok(schedule) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ClassScheduleDto>> Create([FromBody] CreateClassScheduleRequest request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClassScheduleDto>> Update(Guid id, [FromBody] UpdateClassScheduleRequest request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleService.UpdateAsync(id, request, cancellationToken);
        return schedule is not null ? Ok(schedule) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _scheduleService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
