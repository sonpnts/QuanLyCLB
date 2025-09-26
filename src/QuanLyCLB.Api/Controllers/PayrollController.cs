using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyCLB.Application.DTOs;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpPost("generate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PayrollPeriodDto>> Generate([FromBody] GeneratePayrollRequest request, CancellationToken cancellationToken)
    {
        var payroll = await _payrollService.GeneratePayrollAsync(request, cancellationToken);
        return Ok(payroll);
    }

    [HttpGet("instructor/{instructorId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<PayrollPeriodDto>>> GetForInstructor(Guid instructorId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !TryValidateInstructor(instructorId))
        {
            return Forbid();
        }

        var payrolls = await _payrollService.GetPayrollsAsync(instructorId, cancellationToken);
        return Ok(payrolls);
    }

    [HttpGet("{payrollId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PayrollPeriodDto>> GetById(Guid payrollId, CancellationToken cancellationToken)
    {
        var payroll = await _payrollService.GetPayrollByIdAsync(payrollId, cancellationToken);
        return payroll is not null ? Ok(payroll) : NotFound();
    }

    private bool TryValidateInstructor(Guid instructorId)
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var currentId) && currentId == instructorId;
    }
}
