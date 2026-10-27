using NorthWind.DomainLogs.Entities;
using NorthWind.Entities.Interfaces;
namespace NorthWind.Sales.Backend.UseCases.Common;

public sealed class DomainLogger(IDomainLogsRepository repository) : IDomainLogger
{
    public async Task LogInformation(DomainLog log, CancellationToken cancellationToken = default)
    {
        await repository.Add(log, cancellationToken);
        await repository.SaveChanges(cancellationToken);
    }
}
