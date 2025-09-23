using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Api.Authorization;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [HasPermission("Payments", PermissionAction.Create)]
    public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.RecordPaymentAsync(request, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("closeout")]
    [HasPermission("Payments", PermissionAction.Update)]
    public async Task<IActionResult> Closeout([FromBody] CloseoutPaymentRequest request, CancellationToken cancellationToken)
    {
        var total = await _paymentService.CloseoutAsync(request, cancellationToken);
        return Ok(new { total });
    }
}
