using NorthWind.Transactions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
namespace NorthWind.Sales.Backend.DataContexts.EFCore.Services;

public sealed class DomainTransaction(NorthWindContext context) : IDomainTransaction, IAsyncDisposable
{
    private IDbContextTransaction? transaction;
    public async Task BeginTransaction(CancellationToken cancellationToken = default)
    {
        if (transaction is not null) throw new InvalidOperationException("A transaction is already active.");
        transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }
    public async Task CommitTransaction(CancellationToken cancellationToken = default)
    {
        if (transaction is null) throw new InvalidOperationException("There is no active transaction.");
        await transaction.CommitAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }
    public async Task RollbackTransaction(CancellationToken cancellationToken = default)
    {
        if (transaction is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            await transaction.DisposeAsync();
            transaction = null;
        }
        context.ChangeTracker.Clear();
    }
    public async ValueTask DisposeAsync() { if (transaction is not null) await transaction.DisposeAsync(); }
}
