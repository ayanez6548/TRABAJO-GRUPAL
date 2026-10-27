using System.ComponentModel;
using System.Diagnostics;

namespace NorthWind.Sales.WebApi.Extensions;

internal static class DevelopmentBrowser
{
    public static void OpenMailtrapOnStartup(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()
            || !Uri.TryCreate(app.Configuration["DevelopmentTools:MailtrapUrl"], UriKind.Absolute, out var url)
            || url.Scheme != Uri.UriSchemeHttps || url.Host != "mailtrap.io")
            return;

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                using var browser = Process.Start(new ProcessStartInfo(url.AbsoluteUri) { UseShellExecute = true });
                app.Logger.LogInformation("Mailtrap inbox opened: {Url}", url.AbsoluteUri);
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                app.Logger.LogWarning(ex, "Could not open the browser. Open Mailtrap at {Url}", url.AbsoluteUri);
            }
        });
    }
}
