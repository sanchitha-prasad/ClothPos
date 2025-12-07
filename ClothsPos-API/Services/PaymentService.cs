using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentDue>> GetAllPaymentsAsync(string? status = null)
    {
        var query = _context.PaymentDues
            .Include(p => p.Sale)
                .ThenInclude(s => s.Cashier)
                    .ThenInclude(c => c.Role)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        return await query.OrderByDescending(p => p.DueDate).ToListAsync();
    }

    public async Task<IEnumerable<PaymentDue>> GetPendingPaymentsAsync()
    {
        return await _context.PaymentDues
            .Include(p => p.Sale)
                .ThenInclude(s => s.Cashier)
                    .ThenInclude(c => c.Role)
            .Where(p => p.Status == "pending")
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentDue>> GetOverduePaymentsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PaymentDues
            .Include(p => p.Sale)
                .ThenInclude(s => s.Cashier)
                    .ThenInclude(c => c.Role)
            .Where(p => p.Status == "pending" && p.DueDate < today)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }

    public async Task<PaymentDue?> GetPaymentByIdAsync(string id)
    {
        return await _context.PaymentDues
            .Include(p => p.Sale)
                .ThenInclude(s => s.Cashier)
                    .ThenInclude(c => c.Role)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PaymentDue?> MarkAsPaidAsync(string id, DateTime? paymentDate = null)
    {
        var payment = await _context.PaymentDues.FindAsync(id);
        if (payment == null) return null;

        payment.Status = "paid";
        if (paymentDate.HasValue)
        {
            payment.DueDate = paymentDate.Value;
        }

        await _context.SaveChangesAsync();
        return payment;
    }
}


