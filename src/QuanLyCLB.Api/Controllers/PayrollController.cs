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

    [HttpGet("coach/{coachId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<PayrollPeriodDto>>> GetForCoach(Guid coachId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !TryValidateCoach(coachId))
        {
            return Forbid();
        }

        var payrolls = await _payrollService.GetPayrollsAsync(coachId, cancellationToken);
        return Ok(payrolls);
    }

    [HttpGet("{payrollId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PayrollPeriodDto>> GetById(Guid payrollId, CancellationToken cancellationToken)
    {
        var payroll = await _payrollService.GetPayrollByIdAsync(payrollId, cancellationToken);
        return payroll is not null ? Ok(payroll) : NotFound();
    }

    private bool TryValidateCoach(Guid coachId)
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var currentId) && currentId == coachId;
    }
}
