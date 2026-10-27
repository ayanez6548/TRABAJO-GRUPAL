using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Exceptions.Entities;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Frontend.BusinessObjects.Interfaces;
using NorthWind.Sales.Frontend.BusinessObjects.ViewModels;
using NorthWind.Sales.Validators.Entities;
using NorthWind.Validation.Entities;

namespace NorthWind.Tests;

[TestClass]
public sealed class FrontendTests
{
    [TestMethod]
    public async Task CreateOrderViewModel_ValidatesBeforeCallingTheGatewayAndPreservesTheOrder()
    {
        var gateway = new GatewayStub();
        var model = new CreateOrderViewModel(gateway, new ModelValidatorHub<CreateOrderDto>([new CreateOrderDtoValidator()]));
        model.CustomerId = "";
        await Assert.ThrowsExactlyAsync<ValidationException>(() => model.CreateOrder());
        Assert.IsNull(gateway.LastOrder);
        model.CustomerId = "ALFKI";
        model.SelectProduct(model.Details[0], new(1, "Chai", 35m, 20));
        Assert.AreEqual(42, await model.CreateOrder());
        Assert.AreEqual("ALFKI", gateway.LastOrder!.CustomerId);
        Assert.HasCount(1, gateway.LastOrder.OrderDetails);
        Assert.AreEqual(model.Subtotal, gateway.LastOrder.OrderDetails.Sum(d => d.UnitPrice * d.Quantity));
    }

    [TestMethod]
    public void SelectingAndChangingProducts_PreservesQuantityAndRejectsDuplicates()
    {
        var model = new CreateOrderViewModel(new GatewayStub(), new ModelValidatorHub<CreateOrderDto>([new CreateOrderDtoValidator()]));
        var first = model.Details[0];
        model.SelectProduct(first, new(1, "First", 10m, 20));
        var second = new OrderDetailViewModel { Quantity = 3 };
        model.Details.Add(second);
        model.SelectProduct(second, new(2, "Second", 20m, 10));
        Assert.ThrowsExactly<ValidationException>(() => model.SelectProduct(second, new(1, "First", 10m, 20)));
        Assert.AreEqual(2, second.ProductId);
        model.SelectProduct(second, new(3, "Third", 30m, 8));
        Assert.AreEqual((short)3, second.Quantity);
        Assert.AreEqual(100m, model.Subtotal);
        model.Details.Remove(first);
        Assert.AreEqual(90m, model.Subtotal);
    }

    private sealed class GatewayStub : ICreateOrderGateway
    {
        public CreateOrderDto? LastOrder { get; private set; }
        public Task<int> CreateOrder(CreateOrderDto dto) { LastOrder = dto; return Task.FromResult(42); }
        public Task<IReadOnlyList<CustomerDto>> GetCustomers() => Task.FromResult<IReadOnlyList<CustomerDto>>([]);
        public Task<IReadOnlyList<ProductDto>> GetProducts() => Task.FromResult<IReadOnlyList<ProductDto>>([]);
        public Task<PagedResult<CustomerDto>> SearchCustomers(string q, int page, int size, CustomerSearchFilters? filters = null) => throw new NotSupportedException();
        public Task<PagedResult<ProductDto>> SearchProducts(string q, int page, int size, ProductSearchFilters? filters = null) => throw new NotSupportedException();
        public Task<SalesSettingsDto> GetSalesSettings() => Task.FromResult(new SalesSettingsDto(10m, 0.15m));
        public Task<PagedResult<InvoiceSummaryDto>> GetInvoiceHistory(int page, int pageSize) => throw new NotSupportedException();
        public Task<InvoiceDto> GetInvoice(int id) => throw new NotSupportedException();
        public Task<byte[]> GetInvoicePdf(int id) => throw new NotSupportedException();

    }
}
