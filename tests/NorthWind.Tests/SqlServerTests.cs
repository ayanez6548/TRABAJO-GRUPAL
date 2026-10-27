using NorthWind.DomainLogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Exceptions.Entities;
using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
using NorthWind.Sales.Backend.IoC;
namespace NorthWind.Tests;

[TestClass, DoNotParallelize]
public sealed class SqlServerTests
{
    [TestMethod, TestCategory("SqlServer")]
    public async Task RealSqlServer_CommitsOrderAndRollsBackOrderDetailsAndSuccessAuditOnMailFailure()
    {
        var connection = Environment.GetEnvironmentVariable("NORTHWIND_TEST_CONNECTION");
        if (string.IsNullOrEmpty(connection)) Assert.Inconclusive("Define NORTHWIND_TEST_CONNECTION para ejecutar la integración con SQL Server.");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["Database:Provider"] = Environment.GetEnvironmentVariable("NORTHWIND_TEST_PROVIDER") ?? "SqlServer",
            ["ConnectionStrings:NorthWind"] = connection
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNorthWindSalesServices(config);
        var mail = new TestMailService();
        var user = new TestUserService();
        services.RemoveAll<IMailService>(); services.AddSingleton<IMailService>(mail);
        services.RemoveAll<IUserService>(); services.AddSingleton<IUserService>(user);
        await using var provider = services.BuildServiceProvider();
        await using (var setup = provider.CreateAsyncScope())
            await setup.ServiceProvider.GetRequiredService<NorthWindContext>().Database.MigrateAsync();

        int committedId;
        await using (var scope = provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ICreateOrderInputPort>().Handle(OrderTests.Valid(
                new NorthWind.Sales.Entities.Dtos.CreateOrder.CreateOrderDetailDto(1, 35, 3)));
            committedId = scope.ServiceProvider.GetRequiredService<ICreateOrderOutputPort>().OrderId;
        }
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NorthWindContext>();
            var detail = await db.OrderDetails.SingleAsync(d => d.OrderId == committedId);
            Assert.AreEqual((short)3, detail.Quantity);
            Assert.AreEqual(user.UserName, (await db.Orders.SingleAsync(o => o.Id == committedId)).CreatedBy);
        }
        mail.Fail = true;
        await using (var scope = provider.CreateAsyncScope())
        {
            await Assert.ThrowsExactlyAsync<NotificationException>(() => scope.ServiceProvider.GetRequiredService<ICreateOrderInputPort>()
                .Handle(OrderTests.Valid(new(1, 35, 1), new(3, 65, 1), new(4, 75, 1), new(5, 50, 1))));
            Assert.AreEqual(0, scope.ServiceProvider.GetRequiredService<ICreateOrderOutputPort>().OrderId);
        }
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NorthWindContext>();
            Assert.AreEqual(1, await db.Orders.CountAsync(o => o.CreatedBy == user.UserName));
            var logs = await db.DomainLogs.Where(l => l.UserName == user.UserName).ToListAsync();
            Assert.HasCount(4, logs); // inicio y éxito; inicio y cancelación.
            Assert.AreEqual(1, logs.Count(l => l.Information.Contains("created.")));
            Assert.AreEqual(1, logs.Count(l => l.Information.Contains("cancelled.")));
            var cancelledId = int.Parse(logs.Single(l => l.Information.Contains("cancelled.")).Information.Split(' ')[2]);
            Assert.IsFalse(await db.Orders.AnyAsync(o => o.Id == cancelledId));
            Assert.IsFalse(await db.OrderDetails.AnyAsync(d => d.OrderId == cancelledId));
        }
    }
    private sealed class TestUserService : IUserService
    {
        public bool IsAuthenticated => true;
        public string UserName { get; } = $"integration-{Guid.NewGuid():N}@example.test";
        public string FullName => "Prueba SQL";
    }
    private sealed class TestMailService : IMailService
    {
        public bool Fail { get; set; }
        public Task SendMailToAdministrator(string subject, string body, CancellationToken cancellationToken = default) =>
            Fail ? Task.FromException(new NotificationException("Falla de correo simulada para verificar rollback real.")) : Task.CompletedTask;
    }
}
