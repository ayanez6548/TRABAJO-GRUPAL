using NorthWind.Sales.Backend.BusinessObjects.Enums;
namespace NorthWind.Sales.Backend.BusinessObjects.POCOEntities;

public class Order
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = "";
    public string ShipAddress { get; set; } = "";
    public string ShipCity { get; set; } = "";
    public string ShipCountry { get; set; } = "";
    public string? ShipPostalCode { get; set; }
    public ShippingType ShippingType { get; set; } = ShippingType.Road;
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal Discount { get; set; } = 10m;
    public decimal VatRate { get; set; }
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "";
}
