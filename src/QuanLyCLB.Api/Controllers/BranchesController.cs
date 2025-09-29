using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<BranchDto>>> GetAll(CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetAllAsync(cancellationToken);
        return Ok(branches);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetByIdAsync(id, cancellationToken);
        return branch is not null ? Ok(branch) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<BranchDto>> Create([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var branch = await _branchService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BranchDto>> Update(Guid id, [FromBody] UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        var branch = await _branchService.UpdateAsync(id, request, cancellationToken);
        return branch is not null ? Ok(branch) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _branchService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
