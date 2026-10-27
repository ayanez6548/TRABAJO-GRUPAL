using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;

// Each provider owns a migration set; both share the same normalized domain model.
public sealed class PostgreSqlNorthWindContext(DbContextOptions<PostgreSqlNorthWindContext> options) : NorthWindContext(options);

public sealed class PostgreSqlNorthWindContextFactory : IDesignTimeDbContextFactory<PostgreSqlNorthWindContext>
{
    public PostgreSqlNorthWindContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__NorthWind")
            ?? "Host=localhost;Port=54330;Database=northwind;Username=northwind";
        return new(new DbContextOptionsBuilder<PostgreSqlNorthWindContext>().UseNpgsql(connection).Options);
    }
}
