using NorthWind.Sales.Validators.Entities;
using NorthWind.DomainLogs.Entities;
using NorthWind.Transactions.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Events.Entities;
using NorthWind.Exceptions.Entities;
using NorthWind.Entities.Interfaces;
using NorthWind.Validation.Entities;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Backend.BusinessObjects.Events;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;
using NorthWind.Sales.Backend.UseCases.CreateOrder;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.ValidationService.FluentValidation;
namespace NorthWind.Tests;

[TestClass]
public sealed class UseCaseTests
{
    private sealed class Harness : ICommandsRepository, ICreateOrderOutputPort, IDomainTransaction,
        IDomainLogger, IUserService, IDomainEventHub<SpecialOrderCreatedEvent>, IQueriesRepository
    {
        public List<string> Calls { get; } = [];
        public List<DomainLog> Logs { get; } = [];
        public OrderAggregate? Stored { get; private set; }
        public bool FailMail { get; set; }
        public bool IsAuthenticated { get; set; } = true;
        public string UserName => "alumna@example.com";
        public string FullName => "Ana Pérez";
        public int OrderId { get; private set; }
        public decimal? Balance { get; set; } = 0;
        public short Stock { get; set; } = 20;
        public int QueryCount { get; private set; }
        public Task CreateOrder(OrderAggregate order, CancellationToken ct = default) { Calls.Add("create"); Stored = order; return Task.CompletedTask; }
        public Task SaveChanges(CancellationToken ct = default) { Calls.Add("save"); Stored!.Id = 42; return Task.CompletedTask; }
        public Task Handle(OrderAggregate order, CancellationToken ct = default) { Calls.Add("present"); OrderId = order.Id; return Task.CompletedTask; }
        public Task BeginTransaction(CancellationToken ct = default) { Calls.Add("begin"); return Task.CompletedTask; }
        public Task CommitTransaction(CancellationToken ct = default) { Calls.Add("commit"); return Task.CompletedTask; }
        public Task RollbackTransaction(CancellationToken ct = default) { Calls.Add("rollback"); Stored = null; return Task.CompletedTask; }
        public Task LogInformation(DomainLog log, CancellationToken ct = default) { Logs.Add(log); return Task.CompletedTask; }
        public Task Raise(SpecialOrderCreatedEvent e, CancellationToken ct = default)
        {
            Calls.Add("event");
            return FailMail ? Task.FromException(new NotificationException("SMTP no disponible")) : Task.CompletedTask;
        }
        public Task<decimal?> GetCustomerCurrentBalance(string id, CancellationToken ct = default) { QueryCount++; return Task.FromResult(Balance); }
        public Task<IReadOnlyList<ProductUnitsInStock>> GetProductsUnitsInStock(IEnumerable<int> ids, CancellationToken ct = default)
        { QueryCount++; return Task.FromResult<IReadOnlyList<ProductUnitsInStock>>(ids.Where(id => id != 99).Select(id => new ProductUnitsInStock(id, Stock)).ToArray()); }
        public Task<IReadOnlyList<CustomerDto>> GetCustomers(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<ProductDto>> GetProducts(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<PagedResult<CustomerDto>> SearchCustomers(string? q, int page, int size, CancellationToken ct = default, CustomerSearchFilters? filters = null) => throw new NotSupportedException();
        public Task<PagedResult<ProductDto>> SearchProducts(string? q, int page, int size, CancellationToken ct = default, ProductSearchFilters? filters = null) => throw new NotSupportedException();
        public Task<PagedResult<InvoiceSummaryDto>> GetInvoiceHistory(string user, int page, int pageSize, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<InvoiceDto?> GetInvoice(int id, string user, CancellationToken ct = default) => throw new NotSupportedException();
        public CreateOrderInteractor Interactor() => new(this, this, new ModelValidatorHub<CreateOrderDto>([
            new CreateOrderDtoValidator(), new CreateOrderCustomerValidator(this), new CreateOrderProductValidator(this)]), this, this, this, this);
    }
    [TestMethod]
    public async Task CreateOrder_RequiresAuthenticationBeforeDoingWork()
    {
        var h = new Harness { IsAuthenticated = false };
        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => h.Interactor().Handle(OrderTests.Valid()));
        Assert.IsEmpty(h.Calls);
        Assert.AreEqual(0, h.QueryCount);
    }
    [TestMethod]
    public async Task InvalidInput_SkipsDatabaseQueriesAndWrites()
    {
        var h = new Harness();
        await Assert.ThrowsExactlyAsync<ValidationException>(() => h.Interactor().Handle(OrderTests.Valid() with { CustomerId = "" }));
        Assert.AreEqual(0, h.QueryCount);
        Assert.IsEmpty(h.Logs);
    }
    [TestMethod]
    public async Task CreateOrder_CommitsBeforePresentingAndRecordsUser()
    {
        var h = new Harness();
        await h.Interactor().Handle(OrderTests.Valid());
        CollectionAssert.AreEqual(new[] { "begin", "create", "save", "commit", "present" }, h.Calls);
        Assert.AreEqual(42, h.OrderId);
        Assert.AreEqual(h.UserName, h.Stored!.CreatedBy);
        Assert.HasCount(2, h.Logs);
        Assert.IsTrue(h.Logs.All(l => l.UserName == h.UserName));
    }
    [TestMethod]
    public async Task SpecialOrder_MailFailureRollsBackAndDoesNotPresent()
    {
        var h = new Harness { FailMail = true };
        await Assert.ThrowsExactlyAsync<NotificationException>(() => h.Interactor().Handle(OrderTests.Valid(
            new(1, 35, 1), new(3, 65, 1), new(4, 75, 1), new(5, 50, 1))));
        CollectionAssert.AreEqual(new[] { "begin", "create", "save", "event", "rollback" }, h.Calls);
        Assert.IsNull(h.Stored);
        Assert.AreEqual(0, h.OrderId);
        StringAssert.Contains(h.Logs.Last().Information, "cancelled");
    }
    [TestMethod]
    public async Task RepeatedProduct_ValidatesTotalQuantityAgainstStock()
    {
        var h = new Harness { Stock = 3 };
        await Assert.ThrowsExactlyAsync<ValidationException>(() => h.Interactor().Handle(OrderTests.Valid(new CreateOrderDetailDto(1, 35, 2), new(1, 35, 2))));
        Assert.IsEmpty(h.Calls);
    }
    [TestMethod]
    public async Task BusinessValidation_RejectsDebtorMissingCustomerAndMissingProduct()
    {
        var h = new Harness { Balance = 100 };
        await Assert.ThrowsExactlyAsync<ValidationException>(() => h.Interactor().Handle(OrderTests.Valid()));
        h.Balance = null;
        await Assert.ThrowsExactlyAsync<ValidationException>(() => h.Interactor().Handle(OrderTests.Valid()));
        h.Balance = 0;
        await Assert.ThrowsExactlyAsync<ValidationException>(() => h.Interactor().Handle(OrderTests.Valid(new CreateOrderDetailDto(99, 35, 1))));
        Assert.IsEmpty(h.Calls);
    }
    [TestMethod]
    public async Task SpecialOrder_RaisesEventBeforeCommit()
    {
        var h = new Harness();
        await h.Interactor().Handle(OrderTests.Valid(new CreateOrderDetailDto(1, 35, 1), new(3, 65, 1), new(4, 75, 1), new(5, 50, 1)));
        Assert.IsLessThan(h.Calls.IndexOf("commit"), h.Calls.IndexOf("event"));
        Assert.AreEqual(42, h.OrderId);
    }
    [TestMethod]
    public async Task Practice07_DatabaseValidationIdentifiesTheDetailAndItsProperty()
    {
        var h = new Harness { Stock = 0 };
        var validator = new CreateOrderProductValidator(h);
        var errors = await validator.Validate(OrderTests.Valid());
        Assert.AreEqual("OrderDetails[0].Quantity", errors.Single().PropertyName);
        errors = await validator.Validate(OrderTests.Valid(new CreateOrderDetailDto(99, 35, 1)));
        Assert.AreEqual("OrderDetails[0].ProductId", errors.Single().PropertyName);
    }
}
