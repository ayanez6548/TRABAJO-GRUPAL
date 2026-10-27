using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
public sealed class NorthWindContextFactory : IDesignTimeDbContextFactory<NorthWindContext>
{
    public NorthWindContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__NorthWind")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=NorthWindPracticas8;Trusted_Connection=True;TrustServerCertificate=True";
        return new(new DbContextOptionsBuilder<NorthWindContext>().UseSqlServer(connection).Options);
    }
}
