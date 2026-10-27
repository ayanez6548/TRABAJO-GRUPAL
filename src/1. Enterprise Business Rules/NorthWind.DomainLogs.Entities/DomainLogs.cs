using NorthWind.Entities.Interfaces;
namespace NorthWind.DomainLogs.Entities;

public sealed record DomainLog(string Information, string UserName)
{
    public DateTimeOffset DateTime { get; init; } = DateTimeOffset.UtcNow;
}
public interface IDomainLogger { Task LogInformation(DomainLog log, CancellationToken cancellationToken = default); }
public interface IDomainLogsRepository : IUnitOfWork
{
    Task Add(DomainLog log, CancellationToken cancellationToken = default);
}
