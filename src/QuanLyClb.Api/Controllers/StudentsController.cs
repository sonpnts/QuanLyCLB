using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Api.Authorization;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permission.ViewStudents)]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var students = await _studentService.SearchAsync(keyword, cancellationToken);
        return Ok(students);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    [HttpPost]
    [HasPermission(Permission.ManageStudents)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await _studentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permission.ManageStudents)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("Id mismatch");
        }

        var student = await _studentService.UpdateAsync(request, cancellationToken);
        return Ok(student);
    }

    [HttpPost("{id:guid}/status")]
    [HasPermission(Permission.ManageStudents)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStudentStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.StudentId)
        {
            return BadRequest("Id mismatch");
        }

        await _studentService.ChangeStatusAsync(request, cancellationToken);
        return NoContent();
    }
}
