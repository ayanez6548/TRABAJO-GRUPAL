using NorthWind.Sales.Validators.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Exceptions.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Backend.BusinessObjects.Enums;
using NorthWind.Sales.Backend.BusinessObjects.Specifications;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.ValidationService.FluentValidation;
namespace NorthWind.Tests;

[TestClass]
public sealed class OrderTests
{
    internal static CreateOrderDto Valid(params CreateOrderDetailDto[] details) =>
        new("ALFKI", "Main Street 123", "Quito", "Ecuador", "170101", details.Length == 0 ? [new(1, 35m, 1)] : details);

    [TestMethod]
    public void Aggregate_PreservesUniqueProductsAndAppliesDefaults()
    {
        var before = DateTimeOffset.UtcNow;
        var order = OrderAggregate.From(Valid(new CreateOrderDetailDto(1, 35, 5)));
        Assert.HasCount(1, order.OrderDetails);
        Assert.AreEqual((short)5, order.OrderDetails.Single().Quantity);
        Assert.AreEqual(35m, order.OrderDetails.Single().UnitPrice);
        Assert.AreEqual(ShippingType.Road, order.ShippingType);
        Assert.AreEqual(DiscountType.Percentage, order.DiscountType);
        Assert.AreEqual(10m, order.Discount);
        Assert.IsTrue(order.OrderDate >= before && order.OrderDate <= DateTimeOffset.UtcNow);
    }
    [TestMethod]
    public void Aggregate_RejectsQuantityOverflow() => Assert.ThrowsExactly<ValidationException>(() =>
        OrderAggregate.From(Valid(new CreateOrderDetailDto(1, 35, short.MaxValue), new(1, 35, 1))));

    [TestMethod]
    public void Specification_CountsDistinctProducts()
    {
        var specification = new SpecialOrderSpecification();
        Assert.IsFalse(specification.IsSatisfiedBy(OrderAggregate.From(Valid(new CreateOrderDetailDto(1, 35, 3), new(3, 65, 1), new(4, 75, 1)))));
        Assert.IsTrue(specification.IsSatisfiedBy(OrderAggregate.From(Valid(new CreateOrderDetailDto(1, 35, 1), new(3, 65, 1), new(4, 75, 1), new(5, 50, 1)))));
    }
    [TestMethod]
    public async Task Validation_RejectsMissingCollectionAndInvalidNumbers()
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        var errors = await validator.Validate(Valid() with { CustomerId = "!", OrderDetails = null! });
        Assert.IsTrue(errors.Any(e => e.PropertyName == "OrderDetails"));
        Assert.IsTrue(errors.Any(e => e.PropertyName == "CustomerId"));
        errors = await validator.Validate(Valid(new CreateOrderDetailDto(0, -1, 0)));
        Assert.IsTrue(errors.Any(e => e.PropertyName.Contains("ProductId")));
        Assert.IsTrue(errors.Any(e => e.PropertyName.Contains("UnitPrice")));
        Assert.IsTrue(errors.Any(e => e.PropertyName.Contains("Quantity")));
    }
    [TestMethod]
    public async Task Validation_AcceptsSpanishCharactersAndOptionalPostalCode()
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        Assert.IsEmpty(await validator.Validate(Valid() with { ShipCity = "Bogotá", ShipPostalCode = null }));
    }
    [TestMethod]
    public async Task Validation_RejectsAggregateOverflowAndExcessPrecision()
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        Assert.IsNotEmpty(await validator.Validate(Valid(new CreateOrderDetailDto(1, 35, short.MaxValue), new(1, 35, 1))));
        Assert.IsNotEmpty(await validator.Validate(Valid(new CreateOrderDetailDto(1, 1.234m, 1))));
    }
}
