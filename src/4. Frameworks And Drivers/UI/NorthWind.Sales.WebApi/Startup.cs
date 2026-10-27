using System.Security.Cryptography;
using NorthWind.Sales.WebApi.Invoices;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NorthWind.Membership.Backend.AspNetIdentity.Services;
using NorthWind.Sales.Backend.Controllers;
using NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
using NorthWind.Sales.Backend.IoC;
using NorthWind.Sales.WebApi.Extensions;
using NorthWind.Sales.WebApi.ExceptionHandlers;

namespace NorthWind.Sales.WebApi;

public static class Startup
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var jwt = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>() ?? new();
        if (string.IsNullOrWhiteSpace(jwt.SecurityKey) && builder.Environment.IsDevelopment())
        {
            jwt.SecurityKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        if (Encoding.UTF8.GetByteCount(jwt.SecurityKey) < 32 || jwt.ExpireInMinutes is < 1 or > 1440
            || string.IsNullOrWhiteSpace(jwt.ValidIssuer) || string.IsNullOrWhiteSpace(jwt.ValidAudience))
            throw new InvalidOperationException("Configure JwtOptions:SecurityKey (at least 32 bytes), issuer, audience and lifetime (1 to 1440 minutes).");
        builder.Services.Configure<JwtOptions>(o =>
        {
            o.SecurityKey = jwt.SecurityKey; o.ValidIssuer = jwt.ValidIssuer;
            o.ValidAudience = jwt.ValidAudience; o.ExpireInMinutes = jwt.ExpireInMinutes;
        });
        builder.Services.AddNorthWindSalesServices(builder.Configuration);
        var vatRate = builder.Configuration.GetValue<decimal?>("SalesSettings:VatRate") ?? 0.15m;
        if (vatRate is < 0 or > 1) throw new InvalidOperationException("SalesSettings:VatRate must be between 0 and 1.");
        builder.Services.AddSingleton<ISalesSettings>(new SalesSettings(vatRate));
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        builder.Services.AddSingleton<IInvoicePdfExporter, InvoicePdfExporter>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = new()
            {
                ValidateIssuer = true, ValidIssuer = jwt.ValidIssuer,
                ValidateAudience = true, ValidAudience = jwt.ValidAudience,
                ValidateLifetime = true, RequireExpirationTime = true,
                ValidateIssuerSigningKey = true, RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecurityKey)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256], NameClaimType = "name", ClockSkew = TimeSpan.FromSeconds(15)
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(
            builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:5200"])
            .AllowAnyHeader().AllowAnyMethod()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new() { Title = "NorthWind · API de ventas", Version = "v1" });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", Description = "Pega el valor accessToken devuelto por /user/Login." });
            o.AddSecurityRequirement(document => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
        });

        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddProblemDetails();
    }

    public static async Task Configure(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
        {
            app.UseSwagger(); app.UseSwaggerUI();
        }
        app.MapGet("/health", () => Results.Ok(new { status = "ok", framework = "net10.0",
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            os = System.Runtime.InteropServices.RuntimeInformation.OSDescription })).AllowAnonymous();
        app.MapNorthWindSalesEndpoints();
        if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<NorthWindContext>().Database.MigrateAsync();
        }
        app.OpenMailtrapOnStartup();
    }
}

internal sealed record SalesSettings(decimal VatRate) : ISalesSettings;
