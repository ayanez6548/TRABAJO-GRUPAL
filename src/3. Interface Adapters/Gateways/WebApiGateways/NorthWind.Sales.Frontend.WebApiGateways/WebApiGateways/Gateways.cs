using System.Net.Http.Json;
using System.Globalization;
using NorthWind.Sales.Frontend.BusinessObjects.Interfaces;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using SalesEndpoints = NorthWind.Sales.Entities.ValueObjects.Endpoints;
using MemberEndpoints = NorthWind.Membership.Entities.ValueObjects.Endpoints;
namespace NorthWind.Sales.Frontend.WebApiGateways.WebApiGateways;

public sealed class CreateOrderGateway(HttpClient client) : ICreateOrderGateway
{
    public async Task<int> CreateOrder(CreateOrderDto dto)
    {
        using var response = await client.PostAsJsonAsync(SalesEndpoints.CreateOrder, dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }
    public async Task<IReadOnlyList<CustomerDto>> GetCustomers() =>
        await client.GetFromJsonAsync<CustomerDto[]>(SalesEndpoints.Customers) ?? [];
    public async Task<IReadOnlyList<ProductDto>> GetProducts() =>
        await client.GetFromJsonAsync<ProductDto[]>(SalesEndpoints.Products) ?? [];
    public async Task<PagedResult<CustomerDto>> SearchCustomers(string query, int page, int pageSize, CustomerSearchFilters? filters = null) =>
        await client.GetFromJsonAsync<PagedResult<CustomerDto>>($"/catalog/customers/search?q={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}"
            + Parameters(("code", filters?.Code), ("name", filters?.Name), ("minBalance", filters?.MinBalance), ("maxBalance", filters?.MaxBalance))) ?? throw new HttpRequestException("No se pudo realizar la búsqueda.");
    public async Task<PagedResult<ProductDto>> SearchProducts(string query, int page, int pageSize, ProductSearchFilters? filters = null) =>
        await client.GetFromJsonAsync<PagedResult<ProductDto>>($"/catalog/products/search?q={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}"
            + Parameters(("code", filters?.Code), ("name", filters?.Name), ("minPrice", filters?.MinPrice), ("maxPrice", filters?.MaxPrice), ("minStock", filters?.MinStock), ("maxStock", filters?.MaxStock))) ?? throw new HttpRequestException("No se pudo realizar la búsqueda.");
    private static string Parameters(params (string Key, object? Value)[] values) => string.Concat(values
        .Where(parameter => parameter.Value is not null)
        .Select(parameter => "&" + parameter.Key + "=" + Uri.EscapeDataString(Convert.ToString(parameter.Value, CultureInfo.InvariantCulture) ?? "")));
    public async Task<SalesSettingsDto> GetSalesSettings() => await client.GetFromJsonAsync<SalesSettingsDto>("/sales/settings") ?? throw new HttpRequestException("No se pudo cargar la configuración de ventas.");
    public async Task<InvoiceDto> GetInvoice(int orderId) => await client.GetFromJsonAsync<InvoiceDto>($"/orders/{orderId}") ?? throw new HttpRequestException("No se encontró la orden solicitada.");
    public async Task<PagedResult<InvoiceSummaryDto>> GetInvoiceHistory(int page, int pageSize) =>
        await client.GetFromJsonAsync<PagedResult<InvoiceSummaryDto>>($"/orders?page={page}&pageSize={pageSize}") ?? throw new HttpRequestException("No se pudo cargar el historial de facturas.");
    public Task<byte[]> GetInvoicePdf(int orderId) => client.GetByteArrayAsync($"/orders/{orderId}/invoice.pdf");
}
public sealed class MembershipGateway(HttpClient client)
{
    public async Task Register(UserRegistrationDto dto)
    {
        using var response = await client.PostAsJsonAsync(MemberEndpoints.Register, dto);
        response.EnsureSuccessStatusCode();
    }
    public async Task<TokensDto> Login(UserCredentialsDto dto)
    {
        using var response = await client.PostAsJsonAsync(MemberEndpoints.Login, dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokensDto>() ?? throw new HttpRequestException("No se pudo iniciar sesión. Inténtalo nuevamente.");
    }
}
