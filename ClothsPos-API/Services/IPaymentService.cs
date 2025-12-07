using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDue>> GetAllPaymentsAsync(string? status = null);
    Task<IEnumerable<PaymentDue>> GetPendingPaymentsAsync();
    Task<IEnumerable<PaymentDue>> GetOverduePaymentsAsync();
    Task<PaymentDue?> GetPaymentByIdAsync(string id);
    Task<PaymentDue?> MarkAsPaidAsync(string id, DateTime? paymentDate = null);
}


