using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
//[Authorize(Policy = "AdminOnly")]
[Route("api/[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorsController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InstructorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _instructorService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var instructor = await _instructorService.GetByIdAsync(id, cancellationToken);
        return instructor is not null ? Ok(instructor) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<InstructorDto>> Create([FromBody] CreateInstructorRequest request, CancellationToken cancellationToken)
    {
        var instructor = await _instructorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> Update(Guid id, [FromBody] UpdateInstructorRequest request, CancellationToken cancellationToken)
    {
        var instructor = await _instructorService.UpdateAsync(id, request, cancellationToken);
        return instructor is not null ? Ok(instructor) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _instructorService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
