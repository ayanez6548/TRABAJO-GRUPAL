using NorthWind.DomainLogs.Entities;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.Repositories.Entities;
namespace NorthWind.Sales.Backend.Repositories.Interfaces;
public interface INorthWindSalesCommandsDataContext
{
    Task AddOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task AddOrderDetailsAsync(IEnumerable<OrderDetail> details, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ReserveStockAsync(IEnumerable<OrderDetail> details, CancellationToken cancellationToken = default);
}
public interface INorthWindSalesQueriesDataContext
{
    IQueryable<Customer> Customers { get; }
    IQueryable<Product> Products { get; }
    IQueryable<Order> Orders { get; }
    IQueryable<OrderDetail> OrderDetails { get; }
    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);
}
public interface INorthWindDomainLogsDataContext
{
    Task AddLogAsync(DomainLogEntry log, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
