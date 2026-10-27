namespace NorthWind.Sales.Entities.Dtos;
public sealed record CustomerDto(string Id, string Name, decimal CurrentBalance);
public sealed record ProductDto(int Id, string Name, decimal UnitPrice, short UnitsInStock);
public sealed record CustomerSearchFilters(string? Code = null, string? Name = null, decimal? MinBalance = null, decimal? MaxBalance = null);
public sealed record ProductSearchFilters(int? Code = null, string? Name = null, decimal? MinPrice = null, decimal? MaxPrice = null, int? MinStock = null, int? MaxStock = null);
