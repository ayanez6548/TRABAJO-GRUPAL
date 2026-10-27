using NorthWind.DomainLogs.Entities;
using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;
namespace NorthWind.Sales.Backend.Repositories.Repositories;

public sealed class DomainLogsRepository(INorthWindDomainLogsDataContext context) : IDomainLogsRepository
{
    public Task Add(DomainLog log, CancellationToken cancellationToken = default) => context.AddLogAsync(
        new DomainLogEntry { DateTime = log.DateTime, Information = log.Information, UserName = log.UserName }, cancellationToken);
    public Task SaveChanges(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
