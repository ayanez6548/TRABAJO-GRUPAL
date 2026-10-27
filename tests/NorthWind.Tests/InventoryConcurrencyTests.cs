using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Exceptions.Entities;
using NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
using NorthWind.Sales.Backend.IoC;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Transactions.Entities;

namespace NorthWind.Tests;

[TestClass, DoNotParallelize]
public sealed class InventoryConcurrencyTests
{
    [TestMethod, TestCategory("SqlServer")]
    public async Task ConcurrentUpdates_CannotOversell_AndRollbackRestoresInventory()
    {
        var connection = Environment.GetEnvironmentVariable("NORTHWIND_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection)) Assert.Inconclusive("Set NORTHWIND_TEST_CONNECTION to run database concurrency checks.");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["ConnectionStrings:NorthWind"] = connection,
            ["Database:Provider"] = Environment.GetEnvironmentVariable("NORTHWIND_TEST_PROVIDER") ?? "SqlServer"
        }).Build();
        var services = new ServiceCollection().AddLogging().AddNorthWindSalesServices(configuration);
        await using var provider = services.BuildServiceProvider();
        int id;
        await using (var setup = provider.CreateAsyncScope())
        {
            var db = setup.ServiceProvider.GetRequiredService<NorthWindContext>();
            await db.Database.MigrateAsync();
            var product = new Product { Name = "Concurrent sale fixture", UnitPrice = 10m, UnitsInStock = 1 };
            db.Products.Add(product); await db.SaveChangesAsync(); id = product.Id;
        }
        try
        {
            async Task<bool> Reserve()
            {
                await using var scope = provider.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<NorthWindContext>();
                var transaction = scope.ServiceProvider.GetRequiredService<IDomainTransaction>();
                await transaction.BeginTransaction();
                try
                {
                    await db.ReserveStockAsync([new OrderDetail { ProductId = id, UnitPrice = 10m, Quantity = 1 }]);
                    await Task.Delay(50);
                    await transaction.CommitTransaction();
                    return true;
                }
                catch (StockConflictException) { await transaction.RollbackTransaction(); return false; }
            }
            var outcomes = await Task.WhenAll(Reserve(), Reserve());
            Assert.AreEqual(1, outcomes.Count(success => success));
            await using var verify = provider.CreateAsyncScope();
            var context = verify.ServiceProvider.GetRequiredService<NorthWindContext>();
            Assert.AreEqual((short)0, (await context.Products.AsNoTracking().SingleAsync(p => p.Id == id)).UnitsInStock);
            await context.Products.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.UnitsInStock, (short)5));
            var rollback = verify.ServiceProvider.GetRequiredService<IDomainTransaction>();
            await rollback.BeginTransaction();
            await context.ReserveStockAsync([new OrderDetail { ProductId = id, UnitPrice = 10m, Quantity = 2 }]);
            await rollback.RollbackTransaction();
            Assert.AreEqual((short)5, (await context.Products.AsNoTracking().SingleAsync(p => p.Id == id)).UnitsInStock);
            await rollback.BeginTransaction();
            await Assert.ThrowsExactlyAsync<StockConflictException>(() => context.ReserveStockAsync([
                new OrderDetail { ProductId = id, UnitPrice = 1m, Quantity = 1 }]));
            await rollback.RollbackTransaction();
            Assert.AreEqual((short)5, (await context.Products.AsNoTracking().SingleAsync(p => p.Id == id)).UnitsInStock);
        }
        finally
        {
            await using var cleanup = provider.CreateAsyncScope();
            await cleanup.ServiceProvider.GetRequiredService<NorthWindContext>().Products.Where(p => p.Id == id).ExecuteDeleteAsync();
        }
    }
}
