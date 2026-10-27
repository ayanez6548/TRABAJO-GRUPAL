namespace NorthWind.Entities.Interfaces;

public interface IUnitOfWork { Task SaveChanges(CancellationToken cancellationToken = default); }
public interface IUserService
{
    bool IsAuthenticated { get; }
    string UserName { get; }
    string FullName { get; }
}
public interface IMailService
{
    Task SendMailToAdministrator(string subject, string body, CancellationToken cancellationToken = default);
}
