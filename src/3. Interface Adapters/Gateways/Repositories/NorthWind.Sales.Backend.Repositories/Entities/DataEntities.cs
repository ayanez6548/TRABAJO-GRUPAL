using NorthWind.DomainLogs.Entities;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
namespace NorthWind.Sales.Backend.Repositories.Entities;
public sealed class OrderDetail
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }
}
public sealed class Customer
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal CurrentBalance { get; set; }
}
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public short UnitsInStock { get; set; }
    public decimal UnitPrice { get; set; }
}
public sealed class DomainLogEntry
{
    public int Id { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public string Information { get; set; } = "";
    public string UserName { get; set; } = "";
}
