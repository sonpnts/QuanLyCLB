using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
//[Authorize(Policy = "AdminOnly")]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly ITrainingClassService _classService;
    private readonly IScheduleService _scheduleService;

    public ClassesController(ITrainingClassService classService, IScheduleService scheduleService)
    {
        _classService = classService;
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TrainingClassDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _classService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingClassDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _classService.GetByIdAsync(id, cancellationToken);
        return item is not null ? Ok(item) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<TrainingClassDto>> Create([FromBody] CreateTrainingClassRequest request, CancellationToken cancellationToken)
    {
        var item = await _classService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TrainingClassDto>> Update(Guid id, [FromBody] UpdateTrainingClassRequest request, CancellationToken cancellationToken)
    {
        var item = await _classService.UpdateAsync(id, request, cancellationToken);
        return item is not null ? Ok(item) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _classService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{classId:guid}/schedules")]
    public async Task<ActionResult<IReadOnlyCollection<ClassScheduleDto>>> GetSchedules(Guid classId, CancellationToken cancellationToken)
    {
        var result = await _scheduleService.GetByClassAsync(classId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{classId:guid}/schedules")]
    public async Task<ActionResult<IReadOnlyCollection<ClassScheduleDto>>> BulkCreateSchedules(Guid classId, [FromBody] BulkCreateScheduleRequest request, CancellationToken cancellationToken)
    {
        if (classId != request.TrainingClassId)
        {
            return BadRequest("Class id mismatch");
        }

        var schedules = await _scheduleService.BulkCreateAsync(request, cancellationToken);
        return Ok(schedules);
    }
}
