namespace NorthWind.Sales.Backend.BusinessObjects.ValueObjects;
public sealed record OrderDetail(int ProductId, decimal UnitPrice, short Quantity);
public sealed record ProductUnitsInStock(int ProductId, short UnitsInStock);
