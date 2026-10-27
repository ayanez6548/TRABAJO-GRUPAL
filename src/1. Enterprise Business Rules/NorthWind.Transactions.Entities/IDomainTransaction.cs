namespace NorthWind.Transactions.Entities;

public interface IDomainTransaction
{
    Task BeginTransaction(CancellationToken cancellationToken = default);
    Task CommitTransaction(CancellationToken cancellationToken = default);
    Task RollbackTransaction(CancellationToken cancellationToken = default);
}
