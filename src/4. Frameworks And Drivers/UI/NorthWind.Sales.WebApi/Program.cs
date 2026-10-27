var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureServices(builder);
var app = builder.Build();
await Startup.Configure(app);
app.Run();

public partial class Program { }
