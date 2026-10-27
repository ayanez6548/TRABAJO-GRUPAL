using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthWind.Exceptions.Entities;
using NorthWind.Entities.Interfaces;
namespace NorthWind.Sales.Backend.SmtpGateways;

public sealed class SmtpOptions
{
    public string Mode { get; set; } = "PickupDirectory";
    public string PickupDirectory { get; set; } = "App_Data/mail";
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpHostPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUserName { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public string SenderEmail { get; set; } = "northwind@example.com";
    public string AdministratorEmail { get; set; } = "admin@example.com";
}
public sealed class MailService(IOptions<SmtpOptions> options, ILogger<MailService> logger) : IMailService
{
    public async Task SendMailToAdministrator(string subject, string body, CancellationToken cancellationToken = default)
    {
        var o = options.Value;
        try
        {
            using var message = new MailMessage(o.SenderEmail, o.AdministratorEmail, subject, body)
                { SubjectEncoding = Encoding.UTF8, BodyEncoding = Encoding.UTF8 };
            using var client = new SmtpClient();
            if (o.Mode == "PickupDirectory")
            {
                var directory = Path.GetFullPath(o.PickupDirectory);
                Directory.CreateDirectory(directory);
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = directory;
            }
            else if (o.Mode == "Smtp")
            {
                client.Host = o.SmtpHost;
                client.Port = o.SmtpHostPort;
                client.EnableSsl = o.EnableSsl;
                client.Credentials = new NetworkCredential(o.SmtpUserName, o.SmtpPassword);
            }
            else throw new InvalidOperationException("SmtpOptions:Mode must be PickupDirectory or Smtp.");
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Notification generated in {Mode} mode", o.Mode);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify a special order");
            throw new NotificationException("No se pudo notificar al administrador. La orden se canceló y no se guardaron cambios.", ex);
        }
    }
}
