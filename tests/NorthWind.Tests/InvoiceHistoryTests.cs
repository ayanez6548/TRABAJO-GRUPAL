using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;
using NorthWind.Sales.Backend.Repositories.Repositories;

namespace NorthWind.Tests;

[TestClass]
public sealed class InvoiceHistoryTests
{
    [TestMethod]
    public async Task History_FiltersByOwnerBeforeCountingAndPaginatesInStableNewestOrder()
    {
        var repository = new QueriesRepository(new Context());
        var first = await repository.GetInvoiceHistory("owner@example.test", 1, 1);
        var second = await repository.GetInvoiceHistory("owner@example.test", 2, 1);
        var third = await repository.GetInvoiceHistory("owner@example.test", 3, 1);
        Assert.AreEqual(3, first.TotalCount);
        Assert.AreEqual(3, first.TotalPages);
        CollectionAssert.AreEqual(new[] { 4, 3, 1 }, new[] { first.Items.Single().Id, second.Items.Single().Id, third.Items.Single().Id });
        var other = await repository.GetInvoiceHistory("other@example.test", 1, 5);
        Assert.AreEqual(2, other.TotalCount);
        CollectionAssert.AreEquivalent(new[] { 2, 5 }, other.Items.Select(item => item.Id).ToArray());
    }

    [TestMethod]
    public async Task History_UsesSavedPricesAndMatchesTheInvoiceDiscountVatAndRounding()
    {
        var repository = new QueriesRepository(new Context());
        var history = await repository.GetInvoiceHistory("owner@example.test", 1, 5);
        var summary = history.Items.Single(item => item.Id == 1);
        var invoice = await repository.GetInvoice(1, "owner@example.test");
        Assert.IsNotNull(invoice);
        Assert.AreEqual(3.45m, summary.Total);
        Assert.AreEqual(invoice.Total, summary.Total);
        Assert.AreEqual(invoice.Details.Count, summary.ProductsCount);
        Assert.AreEqual("ALFKI", summary.CustomerId);
        Assert.AreEqual("Sample customer", summary.CustomerName);
    }

    [TestMethod]
    public async Task History_RejectsMissingIdentityAndReturnsEmptyForAnOwnerWithoutSales()
    {
        var repository = new QueriesRepository(new Context());
        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => repository.GetInvoiceHistory("", 1, 5));
        var empty = await repository.GetInvoiceHistory("new@example.test", 1, 5);
        Assert.AreEqual(0, empty.TotalCount);
        Assert.IsEmpty(empty.Items);
    }

    private sealed class Context : INorthWindSalesQueriesDataContext
    {
        public IQueryable<Customer> Customers => new[] { new Customer { Id = "ALFKI", Name = "Sample customer" } }.AsQueryable();
        public IQueryable<Product> Products => new[] { new Product { Id = 1, Name = "Sample product", UnitPrice = 999m, UnitsInStock = 10 } }.AsQueryable();
        public IQueryable<Order> Orders => new[] {
            Order(1, "owner@example.test", 1), Order(2, "other@example.test", 2),
            Order(3, "owner@example.test", 3), Order(4, "owner@example.test", 3), Order(5, "other@example.test", 4)
        }.AsQueryable();
        public IQueryable<OrderDetail> OrderDetails => new[] {
            new OrderDetail { OrderId = 1, ProductId = 1, UnitPrice = 1.11m, Quantity = 3 },
            new OrderDetail { OrderId = 2, ProductId = 1, UnitPrice = 30m, Quantity = 2 },
            new OrderDetail { OrderId = 3, ProductId = 1, UnitPrice = 10m, Quantity = 1 },
            new OrderDetail { OrderId = 4, ProductId = 1, UnitPrice = 10m, Quantity = 2 },
            new OrderDetail { OrderId = 5, ProductId = 1, UnitPrice = 40m, Quantity = 1 }
        }.AsQueryable();
        private static Order Order(int id, string owner, int day) => new()
        { Id = id, CustomerId = "ALFKI", CreatedBy = owner, OrderDate = new DateTimeOffset(2026, 1, day, 12, 0, 0, TimeSpan.Zero), Discount = 10m, VatRate = .15m };
        public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => Task.FromResult(query.Count());
        public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => Task.FromResult(query.FirstOrDefault());
        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => Task.FromResult(query.ToList());
    }
}
