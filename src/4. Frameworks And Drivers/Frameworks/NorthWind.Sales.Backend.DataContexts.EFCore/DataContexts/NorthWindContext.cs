using NorthWind.DomainLogs.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NorthWind.Exceptions.Entities;
using NorthWind.Membership.Backend.AspNetIdentity.Entities;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;
namespace NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;

public class NorthWindContext(DbContextOptions options) : IdentityDbContext<NorthWindUser>(options),
    INorthWindSalesCommandsDataContext, INorthWindSalesQueriesDataContext, INorthWindDomainLogsDataContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<DomainLogEntry> DomainLogs => Set<DomainLogEntry>();
    IQueryable<Customer> INorthWindSalesQueriesDataContext.Customers => Customers.AsNoTracking();
    IQueryable<Product> INorthWindSalesQueriesDataContext.Products => Products.AsNoTracking();
    IQueryable<Order> INorthWindSalesQueriesDataContext.Orders => Orders.AsNoTracking();
    IQueryable<OrderDetail> INorthWindSalesQueriesDataContext.OrderDetails => OrderDetails.AsNoTracking();

    public async Task ReserveStockAsync(IEnumerable<OrderDetail> details, CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is null) throw new InvalidOperationException("Inventory updates require an active transaction.");
        // Conditional updates acquire write locks held until commit. Concurrent requests cannot oversell.
        foreach (var line in details.OrderBy(d => d.ProductId))
        {
            int affected = await Products.Where(p => p.Id == line.ProductId && p.UnitsInStock >= line.Quantity
                && p.UnitPrice == line.UnitPrice).ExecuteUpdateAsync(update => update
                    .SetProperty(p => p.UnitsInStock, p => (short)(p.UnitsInStock - line.Quantity)), cancellationToken);
            if (affected != 1)
                throw new StockConflictException($"El producto {line.ProductId} no está disponible o sus existencias o precio cambiaron. Actualiza el catálogo e inténtalo nuevamente.");
        }
    }
    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        EntityFrameworkQueryableExtensions.CountAsync(query, cancellationToken);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(NorthWindContext).Assembly);
        builder.Entity<NorthWindUser>().Property(u => u.FirstName).HasMaxLength(100);
        builder.Entity<NorthWindUser>().Property(u => u.LastName).HasMaxLength(100);
        builder.Entity<NorthWindUser>().HasIndex(u => u.NormalizedEmail).IsUnique();
    }
    public async Task AddOrderAsync(Order order, CancellationToken cancellationToken = default) =>
        await Orders.AddAsync(order, cancellationToken);
    public Task AddOrderDetailsAsync(IEnumerable<OrderDetail> details, CancellationToken cancellationToken = default) =>
        OrderDetails.AddRangeAsync(details, cancellationToken);
    public async Task AddLogAsync(DomainLogEntry log, CancellationToken cancellationToken = default) =>
        await DomainLogs.AddAsync(log, cancellationToken);
    async Task INorthWindDomainLogsDataContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try { await SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) { throw new UpdateException("The audit log could not be saved.", ex); }
    }
    async Task INorthWindSalesCommandsDataContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try { await SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) { throw new UpdateException("The order could not be saved.", ex); }
    }
    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query, cancellationToken);
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        EntityFrameworkQueryableExtensions.ToListAsync(query, cancellationToken);
}
