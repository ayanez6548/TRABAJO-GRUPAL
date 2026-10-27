using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;

namespace NorthWind.Sales.Frontend.BusinessObjects.Interfaces;

public interface ICreateOrderGateway
{
    Task<int> CreateOrder(CreateOrderDto dto);
    Task<IReadOnlyList<CustomerDto>> GetCustomers();
    Task<IReadOnlyList<ProductDto>> GetProducts();
    Task<PagedResult<CustomerDto>> SearchCustomers(string query, int page, int pageSize, CustomerSearchFilters? filters = null);
    Task<PagedResult<ProductDto>> SearchProducts(string query, int page, int pageSize, ProductSearchFilters? filters = null);
    Task<SalesSettingsDto> GetSalesSettings();
    Task<InvoiceDto> GetInvoice(int orderId);
    Task<PagedResult<InvoiceSummaryDto>> GetInvoiceHistory(int page, int pageSize);
    Task<byte[]> GetInvoicePdf(int orderId);
}
