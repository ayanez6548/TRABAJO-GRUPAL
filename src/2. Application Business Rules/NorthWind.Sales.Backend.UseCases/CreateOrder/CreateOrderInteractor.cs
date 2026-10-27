using NorthWind.DomainLogs.Entities;
using NorthWind.Transactions.Entities;
using NorthWind.Events.Entities;
using NorthWind.Exceptions.Entities;
using NorthWind.Entities.Interfaces;
using NorthWind.Validation.Entities;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Backend.BusinessObjects.Events;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.Specifications;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
namespace NorthWind.Sales.Backend.UseCases.CreateOrder;

public sealed class CreateOrderInteractor(ICreateOrderOutputPort outputPort, ICommandsRepository repository,
    IModelValidatorHub<CreateOrderDto> validator, IDomainEventHub<SpecialOrderCreatedEvent> events,
    IDomainLogger logger, IDomainTransaction transaction, IUserService user, ISalesSettings? settings = null) : ICreateOrderInputPort
{
    public async Task Handle(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (!user.IsAuthenticated) throw new UnauthorizedAccessException();
        var errors = await validator.Validate(dto, cancellationToken);
        if (errors.Count > 0) throw new ValidationException(errors);
        await logger.LogInformation(new("Sales order creation started", user.UserName), cancellationToken);
        var order = OrderAggregate.From(dto);
        order.CreatedBy = user.UserName;
        order.VatRate = settings?.VatRate ?? 0.15m;
        try
        {
            await transaction.BeginTransaction(cancellationToken);
            await repository.CreateOrder(order, cancellationToken);
            await repository.SaveChanges(cancellationToken);
            await logger.LogInformation(new($"Sales order {order.Id} created.", user.UserName), cancellationToken);
            if (new SpecialOrderSpecification().IsSatisfiedBy(order))
                await events.Raise(new(order.Id, order.OrderDetails.Count), cancellationToken);
            await transaction.CommitTransaction(cancellationToken);
        }
        catch
        {
            await transaction.RollbackTransaction(CancellationToken.None);
            await logger.LogInformation(new($"Sales order {order.Id} cancelled.", user.UserName), CancellationToken.None);
            throw;
        }
        await outputPort.Handle(order, cancellationToken);
    }
}
