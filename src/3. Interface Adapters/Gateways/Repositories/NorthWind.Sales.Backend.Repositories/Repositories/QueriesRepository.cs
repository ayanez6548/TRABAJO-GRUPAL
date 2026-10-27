using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;
using NorthWind.Sales.Backend.Repositories.Interfaces;
using NorthWind.Sales.Entities.Dtos;
namespace NorthWind.Sales.Backend.Repositories.Repositories;
public sealed class QueriesRepository(INorthWindSalesQueriesDataContext context) : IQueriesRepository
{
    public Task<decimal?> GetCustomerCurrentBalance(string id, CancellationToken cancellationToken = default) =>
        context.FirstOrDefaultAsync(context.Customers.Where(c => c.Id == id).Select(c => (decimal?)c.CurrentBalance), cancellationToken);
    public async Task<IReadOnlyList<ProductUnitsInStock>> GetProductsUnitsInStock(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var keys = ids.ToArray();
        return await context.ToListAsync(context.Products.Where(p => keys.Contains(p.Id))
            .Select(p => new ProductUnitsInStock(p.Id, p.UnitsInStock)), cancellationToken);
    }
    public async Task<IReadOnlyList<CustomerDto>> GetCustomers(CancellationToken cancellationToken = default) =>
        await context.ToListAsync(context.Customers.OrderBy(c => c.Id).Select(c => new CustomerDto(c.Id, c.Name, c.CurrentBalance)), cancellationToken);
    public async Task<IReadOnlyList<ProductDto>> GetProducts(CancellationToken cancellationToken = default) =>
        await context.ToListAsync(context.Products.Where(p => p.UnitsInStock > 0).OrderBy(p => p.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.UnitPrice, p.UnitsInStock)), cancellationToken);

    public async Task<PagedResult<CustomerDto>> SearchCustomers(string? query, int page, int pageSize, CancellationToken cancellationToken = default, CustomerSearchFilters? filters = null)
    {
        var search = (query ?? "").Trim().ToLowerInvariant();
        var source = context.Customers.Where(c => c.Id.ToLower().Contains(search) || c.Name.ToLower().Contains(search));
        if (!string.IsNullOrWhiteSpace(filters?.Code)) { var code = filters.Code.Trim().ToLowerInvariant(); source = source.Where(c => c.Id.ToLower().Contains(code)); }
        if (!string.IsNullOrWhiteSpace(filters?.Name)) { var name = filters.Name.Trim().ToLowerInvariant(); source = source.Where(c => c.Name.ToLower().Contains(name)); }
        if (filters?.MinBalance is decimal minimum) source = source.Where(c => c.CurrentBalance >= minimum);
        if (filters?.MaxBalance is decimal maximum) source = source.Where(c => c.CurrentBalance <= maximum);
        int count = await context.CountAsync(source, cancellationToken);
        var items = await context.ToListAsync(source.OrderBy(c => c.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CustomerDto(c.Id, c.Name, c.CurrentBalance)), cancellationToken);
        return new(items, page, pageSize, count);
    }
    public async Task<PagedResult<ProductDto>> SearchProducts(string? query, int page, int pageSize, CancellationToken cancellationToken = default, ProductSearchFilters? filters = null)
    {
        var search = (query ?? "").Trim().ToLowerInvariant();
        var source = context.Products.Where(p => p.UnitsInStock > 0 && (p.Id.ToString().Contains(search) || p.Name.ToLower().Contains(search)));
        if (filters?.Code is int code) source = source.Where(p => p.Id == code);
        if (!string.IsNullOrWhiteSpace(filters?.Name)) { var name = filters.Name.Trim().ToLowerInvariant(); source = source.Where(p => p.Name.ToLower().Contains(name)); }
        if (filters?.MinPrice is decimal minimumPrice) source = source.Where(p => p.UnitPrice >= minimumPrice);
        if (filters?.MaxPrice is decimal maximumPrice) source = source.Where(p => p.UnitPrice <= maximumPrice);
        if (filters?.MinStock is int minimumStock) source = source.Where(p => p.UnitsInStock >= minimumStock);
        if (filters?.MaxStock is int maximumStock) source = source.Where(p => p.UnitsInStock <= maximumStock);
        int count = await context.CountAsync(source, cancellationToken);
        var items = await context.ToListAsync(source.OrderBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.UnitPrice, p.UnitsInStock)), cancellationToken);
        return new(items, page, pageSize, count);
    }
    public async Task<PagedResult<InvoiceSummaryDto>> GetInvoiceHistory(string userName, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new UnauthorizedAccessException();
        var source = from order in context.Orders
            join customer in context.Customers on order.CustomerId equals customer.Id
            where order.CreatedBy == userName
            select new { order.Id, order.OrderDate, CustomerId = customer.Id, CustomerName = customer.Name, order.Discount, order.VatRate };
        var count = await context.CountAsync(source, cancellationToken);
        var headers = await context.ToListAsync(source.OrderByDescending(order => order.OrderDate).ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize).Take(pageSize), cancellationToken);
        if (headers.Count == 0) return new([], page, pageSize, count);
        var ids = headers.Select(order => order.Id).ToArray();
        var details = await context.ToListAsync(context.OrderDetails.Where(line => ids.Contains(line.OrderId))
            .Select(line => new { line.OrderId, line.UnitPrice, line.Quantity }), cancellationToken);
        var linesByOrder = details.ToLookup(line => line.OrderId);
        var items = headers.Select(header =>
        {
            var lines = linesByOrder[header.Id].ToArray();
            var subtotal = lines.Sum(line => decimal.Round(line.UnitPrice * line.Quantity, 2, MidpointRounding.AwayFromZero));
            return new InvoiceSummaryDto(header.Id, header.OrderDate, header.CustomerId, header.CustomerName,
                lines.Length, InvoiceDto.CalculateTotal(subtotal, header.Discount, header.VatRate));
        }).ToArray();
        return new(items, page, pageSize, count);
    }
    public async Task<InvoiceDto?> GetInvoice(int orderId, string userName, CancellationToken cancellationToken = default)
    {
        var invoice = await context.FirstOrDefaultAsync(
            from order in context.Orders
            join customer in context.Customers on order.CustomerId equals customer.Id
            where order.Id == orderId && order.CreatedBy == userName
            select new InvoiceDto(order.Id, order.OrderDate, customer.Id, customer.Name, order.ShipAddress,
                order.ShipCity, order.ShipCountry, order.ShipPostalCode, order.CreatedBy, order.Discount, order.VatRate,
                Array.Empty<InvoiceLineDto>()), cancellationToken);
        if (invoice is null) return null;
        var details = await context.ToListAsync(
            from line in context.OrderDetails
            join product in context.Products on line.ProductId equals product.Id
            where line.OrderId == orderId
            orderby line.ProductId
            select new InvoiceLineDto(line.ProductId, product.Name, line.UnitPrice, line.Quantity), cancellationToken);
        return invoice with { Details = details };
    }
}
