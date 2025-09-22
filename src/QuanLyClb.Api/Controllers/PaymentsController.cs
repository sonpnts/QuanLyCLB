using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;

namespace QuanLyClb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.RecordPaymentAsync(request, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("closeout")]
    public async Task<IActionResult> Closeout([FromBody] CloseoutPaymentRequest request, CancellationToken cancellationToken)
    {
        var total = await _paymentService.CloseoutAsync(request, cancellationToken);
        return Ok(new { total });
    }
}
