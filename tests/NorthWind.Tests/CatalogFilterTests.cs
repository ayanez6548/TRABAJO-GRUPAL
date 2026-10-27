using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;
using NorthWind.Sales.Backend.Repositories.Repositories;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Frontend.WebApiGateways.WebApiGateways;
using System.Globalization;
using System.Net;

namespace NorthWind.Tests;

[TestClass]
public sealed class CatalogFilterTests
{
    [TestMethod]
    public async Task CustomerFilters_CombineCodeNameAndInclusiveBalanceRange()
    {
        var repository = new QueriesRepository(new Context());
        var result = await repository.SearchCustomers(null, 1, 5, filters: new("a", "  ALPHA ", 10m, 10m));
        CollectionAssert.AreEqual(new[] { "A0002" }, result.Items.Select(item => item.Id).ToArray());
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(0, (await repository.SearchCustomers(null, 1, 5, filters: new("B", "Alpha"))).TotalCount);
    }

    [TestMethod]
    public async Task ProductFilters_CombineFieldsExcludeZeroStockAndPaginateAfterFiltering()
    {
        var repository = new QueriesRepository(new Context());
        var filter = new ProductSearchFilters(Name: "TEA", MinPrice: 10m, MaxPrice: 20m, MinStock: 1, MaxStock: 10);
        var first = await repository.SearchProducts(null, 1, 1, filters: filter);
        var second = await repository.SearchProducts(null, 2, 1, filters: filter);
        Assert.AreEqual(2, first.TotalCount);
        Assert.AreEqual(2, first.TotalPages);
        Assert.AreEqual(1, first.Items.Single().Id);
        Assert.AreEqual(2, second.Items.Single().Id);
        Assert.AreEqual(0, (await repository.SearchProducts(null, 1, 5, filters: filter with { Code = 3 })).TotalCount);
        Assert.AreEqual(2, (await repository.SearchProducts(null, 1, 5, filters: filter with { Code = 2 })).Items.Single().Id);
    }

    [TestMethod]
    public async Task ExistingCombinedSearch_RemainsCompatibleWithFieldFilters()
    {
        var repository = new QueriesRepository(new Context());
        Assert.AreEqual(2, (await repository.SearchCustomers("alpha", 1, 5)).TotalCount);
        Assert.AreEqual(1, (await repository.SearchProducts("tea", 1, 5, filters: new(MinPrice: 20m))).TotalCount);
    }

    [TestMethod]
    public async Task Gateway_EncodesFieldValuesAndUsesInvariantDecimalSeparators()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-EC");
            var handler = new Handler();
            var gateway = new CreateOrderGateway(new HttpClient(handler) { BaseAddress = new Uri("http://localhost") });
            await gateway.SearchProducts("", 2, 5, new(Name: "Tea & Coffee", MinPrice: 12.5m, MaxStock: 20));
            StringAssert.Contains(handler.Uri!.Query, "minPrice=12.5");
            StringAssert.Contains(handler.Uri.Query, "name=Tea%20%26%20Coffee");
            StringAssert.Contains(handler.Uri.Query, "page=2");
            StringAssert.Contains(handler.Uri.Query, "maxStock=20");
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    private sealed class Handler : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("{\"items\":[],\"page\":2,\"pageSize\":5,\"totalCount\":0}", System.Text.Encoding.UTF8, "application/json") });
        }
    }

    private sealed class Context : INorthWindSalesQueriesDataContext
    {
        public IQueryable<Customer> Customers => new[] {
            new Customer { Id = "A0001", Name = "Alpha Market", CurrentBalance = 0 },
            new Customer { Id = "A0002", Name = "Alpha Shop", CurrentBalance = 10 },
            new Customer { Id = "B0001", Name = "Beta Market", CurrentBalance = 10 }
        }.AsQueryable();
        public IQueryable<Product> Products => new[] {
            new Product { Id = 1, Name = "Green Tea", UnitPrice = 10, UnitsInStock = 1 },
            new Product { Id = 2, Name = "Black Tea", UnitPrice = 20, UnitsInStock = 10 },
            new Product { Id = 3, Name = "Tea Empty", UnitPrice = 10, UnitsInStock = 0 },
            new Product { Id = 4, Name = "Coffee", UnitPrice = 10, UnitsInStock = 5 },
            new Product { Id = 5, Name = "Tea Bulk", UnitPrice = 10, UnitsInStock = 11 }
        }.AsQueryable();
        public IQueryable<Order> Orders => Array.Empty<Order>().AsQueryable();
        public IQueryable<OrderDetail> OrderDetails => Array.Empty<OrderDetail>().AsQueryable();
        public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => Task.FromResult(query.Count());
        public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => Task.FromResult(query.FirstOrDefault());
        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) => Task.FromResult(query.ToList());
    }
}
