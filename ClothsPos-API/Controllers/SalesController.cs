using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClothsPosAPI.Services;
using ClothsPosAPI.Models;
using ClothsPosAPI.DTOs;
using System.Linq;

namespace ClothsPosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(ISaleService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSales([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int page = 1, [FromQuery] int limit = 100)
    {
        try
        {
            var sales = await _saleService.GetAllSalesAsync(startDate, endDate, page, limit);
            var result = sales.Select(s => new
            {
                id = s.Id,
                date = s.Date.ToString("yyyy-MM-dd"),
                total = s.Total,
                tax = s.Tax,
                subtotal = s.Subtotal,
                paymentMethod = s.PaymentMethod,
                status = s.Status,
                customerName = s.CustomerName,
                cashierId = s.CashierId,
                cashier = s.Cashier != null ? new { id = s.Cashier.Id, username = s.Cashier.Username, roleName = s.Cashier.Role?.Name } : null,
                items = s.SaleItems.Select(si => new
                {
                    id = si.Id,
                    itemId = si.ItemId,
                    item = si.Item != null ? new { id = si.Item.Id, name = si.Item.Name } : null,
                    quantity = si.Quantity,
                    price = si.Price,
                    total = si.Total
                })
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sales");
            return StatusCode(500, new { message = "Error fetching sales", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSale(string id)
    {
        try
        {
            var sale = await _saleService.GetSaleByIdAsync(id);
            if (sale == null)
            {
                return NotFound(new { message = "Sale not found" });
            }

            return Ok(new
            {
                id = sale.Id,
                date = sale.Date.ToString("yyyy-MM-dd"),
                total = sale.Total,
                tax = sale.Tax,
                subtotal = sale.Subtotal,
                paymentMethod = sale.PaymentMethod,
                status = sale.Status,
                customerName = sale.CustomerName,
                cashierId = sale.CashierId,
                cashier = sale.Cashier != null ? new { id = sale.Cashier.Id, username = sale.Cashier.Username } : null,
                items = sale.SaleItems.Select(si => new
                {
                    id = si.Id,
                    itemId = si.ItemId,
                    item = si.Item != null ? new { id = si.Item.Id, name = si.Item.Name } : null,
                    quantity = si.Quantity,
                    price = si.Price,
                    total = si.Total
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sale");
            return StatusCode(500, new { message = "Error fetching sale", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateSale([FromBody] CreateSaleDto saleDto)
    {
        try
        {
            // Map DTO to Sale model (backend will generate IDs)
            var sale = new Sale
            {
                Date = saleDto.Date,
                Subtotal = saleDto.Subtotal,
                Tax = saleDto.Tax,
                Total = saleDto.Total,
                PaymentMethod = saleDto.PaymentMethod,
                Status = saleDto.Status,
                CustomerName = saleDto.CustomerName,
                CashierId = saleDto.CashierId,
                SaleItems = saleDto.SaleItems.Select(si => new SaleItem
                {
                    ItemId = si.ItemId,
                    Quantity = si.Quantity,
                    Price = si.Price,
                    Total = si.Total,
                    // SaleId will be set by the service
                }).ToList()
            };
            
            var createdSale = await _saleService.CreateSaleAsync(sale);
            
            // Return a DTO to avoid circular reference issues
            // Create a completely new object with only primitive values
            var saleItemsDto = new List<object>();
            if (createdSale.SaleItems != null)
            {
                foreach (var si in createdSale.SaleItems)
                {
                    saleItemsDto.Add(new
                    {
                        id = si.Id,
                        saleId = si.SaleId,
                        itemId = si.ItemId,
                        quantity = si.Quantity,
                        price = si.Price,
                        total = si.Total
                    });
                }
            }
            
            var saleResponse = new
            {
                id = createdSale.Id,
                date = createdSale.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                total = createdSale.Total,
                tax = createdSale.Tax,
                subtotal = createdSale.Subtotal,
                paymentMethod = createdSale.PaymentMethod,
                status = createdSale.Status,
                customerName = createdSale.CustomerName,
                cashierId = createdSale.CashierId,
                saleItems = saleItemsDto
            };
            
            return CreatedAtAction(nameof(GetSale), new { id = createdSale.Id }, saleResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale");
            
            // Get full error details including inner exceptions
            var errorDetails = ex.Message;
            if (ex.InnerException != null)
            {
                errorDetails += $" | Inner: {ex.InnerException.Message}";
            }
            
            return StatusCode(500, new { 
                message = "Error creating sale", 
                error = errorDetails,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpPost("{id}/refund")]
    public async Task<IActionResult> RefundSale(string id)
    {
        try
        {
            var result = await _saleService.RefundSaleAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Sale not found or already refunded" });
            }
            return Ok(new { message = "Sale refunded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding sale");
            return StatusCode(500, new { message = "Error refunding sale", error = ex.Message });
        }
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> VoidSale(string id)
    {
        try
        {
            var result = await _saleService.VoidSaleAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Sale not found or cannot be voided" });
            }
            return Ok(new { message = "Sale voided successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding sale");
            return StatusCode(500, new { message = "Error voiding sale", error = ex.Message });
        }
    }

    [HttpGet("reports")]
    public async Task<ActionResult<object>> GetSalesReport([FromQuery] string period = "daily", [FromQuery] DateTime? date = null)
    {
        try
        {
            var report = await _saleService.GetSalesReportAsync(period, date);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sales report");
            return StatusCode(500, new { message = "Error fetching sales report", error = ex.Message });
        }
    }
}


