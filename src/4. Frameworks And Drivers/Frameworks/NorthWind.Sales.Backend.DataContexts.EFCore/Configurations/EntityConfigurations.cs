using NorthWind.DomainLogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.Repositories.Entities;
namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.Property(o => o.CustomerId).IsRequired().HasMaxLength(5).IsFixedLength();
        b.Property(o => o.ShipAddress).IsRequired().HasMaxLength(60);
        b.Property(o => o.ShipCity).IsRequired().HasMaxLength(15);
        b.Property(o => o.ShipCountry).IsRequired().HasMaxLength(15);
        b.Property(o => o.ShipPostalCode).HasMaxLength(10);
        b.Property(o => o.Discount).HasPrecision(8, 2);
        b.Property(o => o.VatRate).HasPrecision(5, 4).HasDefaultValue(0m);
        b.Property(o => o.CreatedBy).IsRequired().HasMaxLength(256);
        b.HasOne<Customer>().WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> b)
    {
        b.HasKey(d => new { d.OrderId, d.ProductId });
        b.Property(d => d.UnitPrice).HasPrecision(8, 2);
        b.HasOne(d => d.Order).WithMany().HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Product>().WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.Property(c => c.Id).HasMaxLength(5).IsFixedLength();
        b.Property(c => c.Name).IsRequired().HasMaxLength(100);
        b.Property(c => c.CurrentBalance).HasPrecision(12, 2);
        b.HasData(new Customer { Id = "ALFKI", Name = "Alfreds Futterkiste", CurrentBalance = 0 },
            new Customer { Id = "ANATR", Name = "Ana Trujillo Emparedados y helados", CurrentBalance = 0 },
            new Customer { Id = "ANTON", Name = "Antonio Moreno Taquería", CurrentBalance = 100 });
    }
}
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.Property(p => p.Name).IsRequired().HasMaxLength(40);
        b.Property(p => p.UnitPrice).HasPrecision(8, 2);
        b.ToTable(t => t.HasCheckConstraint("CK_Products_NonNegativeStock", "\"UnitsInStock\" >= 0"));
        b.HasData(new Product { Id = 1, Name = "Chai", UnitPrice = 35, UnitsInStock = 20 },
            new Product { Id = 2, Name = "Chang", UnitPrice = 55, UnitsInStock = 0 },
            new Product { Id = 3, Name = "Aniseed Syrup", UnitPrice = 65, UnitsInStock = 20 },
            new Product { Id = 4, Name = "Chef Anton's Cajun Seasoning", UnitPrice = 75, UnitsInStock = 40 },
            new Product { Id = 5, Name = "Chef Anton's Gumbo Mix", UnitPrice = 50, UnitsInStock = 20 });
    }
}
public sealed class DomainLogConfiguration : IEntityTypeConfiguration<DomainLogEntry>
{
    public void Configure(EntityTypeBuilder<DomainLogEntry> b)
    {
        b.Property(l => l.Information).IsRequired().HasMaxLength(1000);
        b.Property(l => l.UserName).IsRequired().HasMaxLength(256);
    }
}
