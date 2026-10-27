namespace NorthWind.Events.Entities;

public interface IDomainEvent;
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}
public interface IDomainEventHub<in T> where T : IDomainEvent
{
    Task Raise(T domainEvent, CancellationToken cancellationToken = default);
}
public sealed class DomainEventHub<T>(IEnumerable<IDomainEventHandler<T>> handlers) : IDomainEventHub<T> where T : IDomainEvent
{
    public async Task Raise(T domainEvent, CancellationToken cancellationToken = default)
    {
        foreach (var handler in handlers) await handler.Handle(domainEvent, cancellationToken);
    }
}
