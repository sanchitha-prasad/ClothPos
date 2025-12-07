using ClothsPosAPI.Models;

namespace ClothsPosAPI.Services;

public interface ISaleService
{
    Task<IEnumerable<Sale>> GetAllSalesAsync(DateTime? startDate = null, DateTime? endDate = null, int page = 1, int limit = 100);
    Task<Sale?> GetSaleByIdAsync(string id);
    Task<Sale> CreateSaleAsync(Sale sale);
    Task<bool> RefundSaleAsync(string saleId);
    Task<bool> VoidSaleAsync(string saleId);
    Task<SalesReport> GetSalesReportAsync(string period, DateTime? date = null);
}


