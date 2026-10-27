namespace NorthWind.Sales.Entities.Dtos;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);
}

public sealed record InvoiceLineDto(int ProductId, string ProductName, decimal UnitPrice, short Quantity)
{
    public decimal Amount => decimal.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);
}

public sealed record InvoiceSummaryDto(int Id, DateTimeOffset OrderDate, string CustomerId,
    string CustomerName, int ProductsCount, decimal Total);

public sealed record InvoiceDto(int Id, DateTimeOffset OrderDate, string CustomerId, string CustomerName,
    string ShipAddress, string ShipCity, string ShipCountry, string? ShipPostalCode, string CreatedBy,
    decimal DiscountPercent, decimal VatRate, IReadOnlyList<InvoiceLineDto> Details)
{
    public decimal Subtotal => Details.Sum(line => line.Amount);
    public decimal DiscountAmount => Round(Subtotal * DiscountPercent / 100m);
    public decimal TaxableAmount => Subtotal - DiscountAmount;
    public decimal VatAmount => Round(TaxableAmount * VatRate);
    public decimal Total => CalculateTotal(Subtotal, DiscountPercent, VatRate);
    public static decimal CalculateTotal(decimal subtotal, decimal discountPercent, decimal vatRate)
    {
        var taxable = subtotal - Round(subtotal * discountPercent / 100m);
        return taxable + Round(taxable * vatRate);
    }
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}

public sealed record SalesSettingsDto(decimal DiscountPercent, decimal VatRate);
