using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.Services;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPayments([FromQuery] string? status)
    {
        try
        {
            var payments = await _paymentService.GetAllPaymentsAsync(status);
            var result = payments.Select(p => new
            {
                id = p.Id,
                saleId = p.SaleId,
                sale = p.Sale != null ? new
                {
                    id = p.Sale.Id,
                    date = p.Sale.Date.ToString("yyyy-MM-dd"),
                    total = p.Sale.Total,
                    cashier = p.Sale.Cashier != null ? new { id = p.Sale.Cashier.Id, username = p.Sale.Cashier.Username, roleName = p.Sale.Cashier.Role?.Name } : null
                } : null,
                amount = p.Amount,
                dueDate = p.DueDate.ToString("yyyy-MM-dd"),
                status = p.Status
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payments");
            return StatusCode(500, new { message = "Error fetching payments", error = ex.Message });
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<object>>> GetPendingPayments()
    {
        try
        {
            var payments = await _paymentService.GetPendingPaymentsAsync();
            var result = payments.Select(p => new
            {
                id = p.Id,
                saleId = p.SaleId,
                sale = p.Sale != null ? new
                {
                    id = p.Sale.Id,
                    date = p.Sale.Date.ToString("yyyy-MM-dd"),
                    total = p.Sale.Total
                } : null,
                amount = p.Amount,
                dueDate = p.DueDate.ToString("yyyy-MM-dd"),
                status = p.Status
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending payments");
            return StatusCode(500, new { message = "Error fetching pending payments", error = ex.Message });
        }
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<object>>> GetOverduePayments()
    {
        try
        {
            var payments = await _paymentService.GetOverduePaymentsAsync();
            var result = payments.Select(p => new
            {
                id = p.Id,
                saleId = p.SaleId,
                sale = p.Sale != null ? new
                {
                    id = p.Sale.Id,
                    date = p.Sale.Date.ToString("yyyy-MM-dd"),
                    total = p.Sale.Total
                } : null,
                amount = p.Amount,
                dueDate = p.DueDate.ToString("yyyy-MM-dd"),
                status = p.Status
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching overdue payments");
            return StatusCode(500, new { message = "Error fetching overdue payments", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetPayment(string id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            return Ok(new
            {
                id = payment.Id,
                saleId = payment.SaleId,
                sale = payment.Sale != null ? new
                {
                    id = payment.Sale.Id,
                    date = payment.Sale.Date.ToString("yyyy-MM-dd"),
                    total = payment.Sale.Total,
                    cashier = payment.Sale.Cashier != null ? new { id = payment.Sale.Cashier.Id, username = payment.Sale.Cashier.Username, roleName = payment.Sale.Cashier.Role?.Name } : null
                } : null,
                amount = payment.Amount,
                dueDate = payment.DueDate.ToString("yyyy-MM-dd"),
                status = payment.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment");
            return StatusCode(500, new { message = "Error fetching payment", error = ex.Message });
        }
    }

    [HttpPost("{id}/paid")]
    public async Task<ActionResult<object>> MarkAsPaid(string id, [FromBody] DateTime? paymentDate = null)
    {
        try
        {
            var payment = await _paymentService.MarkAsPaidAsync(id, paymentDate);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            return Ok(new
            {
                id = payment.Id,
                saleId = payment.SaleId,
                amount = payment.Amount,
                dueDate = payment.DueDate.ToString("yyyy-MM-dd"),
                status = payment.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment as paid");
            return StatusCode(500, new { message = "Error marking payment as paid", error = ex.Message });
        }
    }
}


