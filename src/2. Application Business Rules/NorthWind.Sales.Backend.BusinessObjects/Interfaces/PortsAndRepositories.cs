using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;
namespace NorthWind.Sales.Backend.BusinessObjects.Interfaces;

public interface ICreateOrderInputPort { Task Handle(CreateOrderDto dto, CancellationToken cancellationToken = default); }
public interface ICreateOrderOutputPort
{
    int OrderId { get; }
    Task Handle(OrderAggregate order, CancellationToken cancellationToken = default);
}
public interface ICommandsRepository : IUnitOfWork
{
    Task CreateOrder(OrderAggregate order, CancellationToken cancellationToken = default);
}
public interface IQueriesRepository
{
    Task<decimal?> GetCustomerCurrentBalance(string customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductUnitsInStock>> GetProductsUnitsInStock(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetCustomers(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetProducts(CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerDto>> SearchCustomers(string? query, int page, int pageSize, CancellationToken cancellationToken = default, CustomerSearchFilters? filters = null);
    Task<PagedResult<ProductDto>> SearchProducts(string? query, int page, int pageSize, CancellationToken cancellationToken = default, ProductSearchFilters? filters = null);
    Task<InvoiceDto?> GetInvoice(int orderId, string userName, CancellationToken cancellationToken = default);
    Task<PagedResult<InvoiceSummaryDto>> GetInvoiceHistory(string userName, int page, int pageSize, CancellationToken cancellationToken = default);
}
