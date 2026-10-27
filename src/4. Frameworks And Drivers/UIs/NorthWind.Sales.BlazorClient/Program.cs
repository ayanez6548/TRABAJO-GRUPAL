using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NorthWind.Sales.BlazorClient;
using NorthWind.Sales.Frontend.IoC;

var culture = System.Globalization.CultureInfo.GetCultureInfo("es-EC");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddNorthWindFrontendServices(builder.Configuration);
await builder.Build().RunAsync();
