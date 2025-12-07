using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;
using Microsoft.Extensions.Logging;

namespace ClothsPosAPI.Services;

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _context;

    public SaleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync(DateTime? startDate = null, DateTime? endDate = null, int page = 1, int limit = 100)
    {
        var query = _context.Sales
            .Include(s => s.Cashier)
                .ThenInclude(c => c.Role)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Item)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(s => s.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.Date <= endDate.Value);
        }

        return await query
            .OrderByDescending(s => s.Date)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Sale?> GetSaleByIdAsync(string id)
    {
        return await _context.Sales
            .Include(s => s.Cashier)
                .ThenInclude(c => c.Role)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Item)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        // Validate Cashier exists
        if (string.IsNullOrEmpty(sale.CashierId))
        {
            throw new InvalidOperationException("CashierId is required");
        }

        var cashier = await _context.Users.FindAsync(sale.CashierId);
        if (cashier == null)
        {
            throw new InvalidOperationException($"Cashier with ID {sale.CashierId} not found");
        }

        // Ensure Sale has an ID
        if (string.IsNullOrEmpty(sale.Id))
        {
            sale.Id = Guid.NewGuid().ToString();
        }

        // Clear navigation properties to avoid tracking issues
        sale.Cashier = null;

        // Set SaleId on all SaleItems (EF will handle this, but validation requires it)
        foreach (var saleItem in sale.SaleItems)
        {
            // Ensure SaleItem has an ID
            if (string.IsNullOrEmpty(saleItem.Id))
            {
                saleItem.Id = Guid.NewGuid().ToString();
            }
            
            // Set SaleId if not already set
            if (string.IsNullOrEmpty(saleItem.SaleId))
            {
                saleItem.SaleId = sale.Id;
            }

            // Clear navigation properties to avoid tracking issues
            saleItem.Sale = null;
            saleItem.Item = null;
        }

        // Validate stock availability and deduct stock for each item
        foreach (var saleItem in sale.SaleItems)
        {
            var item = await _context.Items.FindAsync(saleItem.ItemId);
            if (item == null)
            {
                throw new InvalidOperationException($"Item with ID {saleItem.ItemId} not found");
            }

            if (item.Stock < saleItem.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for item {item.Name}. Available: {item.Stock}, Requested: {saleItem.Quantity}");
            }

            // Deduct stock
            item.Stock -= saleItem.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
        }

        // Calculate totals if not already set
        if (sale.Subtotal == 0)
        {
            sale.Subtotal = sale.SaleItems.Sum(si => si.Total);
        }

        if (sale.Total == 0)
        {
            sale.Total = sale.Subtotal + sale.Tax;
        }

        // Ensure Date is set
        if (sale.Date == default)
        {
            sale.Date = DateTime.UtcNow;
        }

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
        
        // Create PaymentDue if sale status is "pending" (not fully paid)
        // Use case-insensitive comparison to handle any case variations
        if (sale.Status != null && sale.Status.Equals("pending", StringComparison.OrdinalIgnoreCase))
        {
            var paymentDue = new PaymentDue
            {
                Id = Guid.NewGuid().ToString(),
                SaleId = sale.Id,
                Amount = sale.Total, // Full amount is due
                DueDate = DateTime.UtcNow.AddDays(7), // Default 7 days from now
                Status = "pending"
            };
            
            _context.PaymentDues.Add(paymentDue);
            await _context.SaveChangesAsync();
        }
        
        // Detach the entity to prevent further tracking
        _context.Entry(sale).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        
        // Ensure all navigation properties are null to prevent circular references
        sale.Cashier = null;
        foreach (var item in sale.SaleItems)
        {
            item.Sale = null;
            item.Item = null;
        }
        
        return sale;
    }

    public async Task<bool> RefundSaleAsync(string saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Item)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null || sale.Status == "refunded")
        {
            return false;
        }

        // Restore stock for each item
        foreach (var saleItem in sale.SaleItems)
        {
            if (saleItem.Item != null)
            {
                saleItem.Item.Stock += saleItem.Quantity;
                saleItem.Item.UpdatedAt = DateTime.UtcNow;
            }
        }

        sale.Status = "refunded";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VoidSaleAsync(string saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Item)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null || sale.Status == "voided" || sale.Status == "refunded")
        {
            return false;
        }

        // Restore stock for each item
        foreach (var saleItem in sale.SaleItems)
        {
            if (saleItem.Item != null)
            {
                saleItem.Item.Stock += saleItem.Quantity;
                saleItem.Item.UpdatedAt = DateTime.UtcNow;
            }
        }

        sale.Status = "voided";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SalesReport> GetSalesReportAsync(string period, DateTime? date = null)
    {
        var reportDate = date ?? DateTime.UtcNow;
        DateTime startDate;
        DateTime endDate = reportDate.Date.AddDays(1).AddTicks(-1);

        switch (period.ToLower())
        {
            case "daily":
                startDate = reportDate.Date;
                break;
            case "weekly":
                startDate = reportDate.Date.AddDays(-(int)reportDate.DayOfWeek);
                break;
            case "monthly":
                startDate = new DateTime(reportDate.Year, reportDate.Month, 1);
                break;
            default:
                startDate = reportDate.Date;
                break;
        }

        var sales = await _context.Sales
            .Where(s => s.Date >= startDate && s.Date <= endDate && s.Status == "completed")
            .ToListAsync();

        var totalSales = sales.Sum(s => s.Total);
        var totalOrders = sales.Count;
        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        var tax = sales.Sum(s => s.Tax);
        var refunds = await _context.Sales
            .Where(s => s.Date >= startDate && s.Date <= endDate && s.Status == "refunded")
            .SumAsync(s => s.Total);

        return new SalesReport
        {
            Period = period,
            Date = reportDate.ToString("yyyy-MM-dd"),
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue,
            Tax = tax,
            Refunds = refunds
        };
    }
}

public class SalesReport
{
    public string Period { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal Tax { get; set; }
    public decimal Refunds { get; set; }
}


