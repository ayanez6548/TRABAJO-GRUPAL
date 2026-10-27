using NorthWind.Exceptions.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Frontend.BusinessObjects.Interfaces;
namespace NorthWind.Sales.Frontend.BusinessObjects.ViewModels;

public sealed class CreateOrderViewModel(ICreateOrderGateway gateway, IModelValidatorHub<CreateOrderDto> validator)
{
    public string CustomerId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string ShipAddress { get; set; } = "Calle Principal 123";
    public string ShipCity { get; set; } = "Quito";
    public string ShipCountry { get; set; } = "Ecuador";
    public string? ShipPostalCode { get; set; } = "170101";
    public List<OrderDetailViewModel> Details { get; } = [new()];
    public decimal Subtotal => Details.Sum(d => d.UnitPrice * d.Quantity);
    public decimal DiscountPercent { get; set; } = 10m;
    public decimal VatRate { get; set; } = 0.15m;
    public decimal DiscountAmount => Round(Subtotal * DiscountPercent / 100m);
    public decimal TaxableAmount => Subtotal - DiscountAmount;
    public decimal VatAmount => Round(TaxableAmount * VatRate);
    public decimal Total => TaxableAmount + VatAmount;
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    public void SelectProduct(OrderDetailViewModel line, ProductDto product)
    {
        if (Details.Any(other => other != line && other.ProductId == product.Id))
            throw new ValidationException([new("OrderDetails", "Este producto ya está en la orden. Modifica su cantidad.")]);
        line.ProductId = product.Id; line.ProductName = product.Name;
        line.UnitPrice = product.UnitPrice; line.UnitsInStock = product.UnitsInStock;
    }
    public async Task<int> CreateOrder()
    {
        var dto = new CreateOrderDto(CustomerId, ShipAddress, ShipCity, ShipCountry, ShipPostalCode,
            Details.Select(d => new CreateOrderDetailDto(d.ProductId, d.UnitPrice, d.Quantity)).ToArray());
        var errors = (await validator.Validate(dto)).ToList();
        for (int index = 0; index < Details.Count; index++)
            if (Details[index].Quantity > Details[index].UnitsInStock)
                errors.Add(new($"OrderDetails[{index}].Quantity", "La cantidad solicitada supera las existencias disponibles."));
        if (errors.Count > 0) throw new ValidationException(errors);
        return await gateway.CreateOrder(dto);
    }
}
public sealed class OrderDetailViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public short UnitsInStock { get; set; }
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; } = 1;
}
