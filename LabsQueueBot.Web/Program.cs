using LabsQueueBot.BusinessLogic;
using LabsQueueBot.Web.ServiceCollectionExtensions;
using Serilog;

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var builder = Host.CreateApplicationBuilder();

builder.Services
    .ConfigureSettings(builder.Configuration)
    .AddSerilog(loggerConfiguration =>
    {
        loggerConfiguration
            .Enrich.WithCorrelationId()
            .ReadFrom.Configuration(builder.Configuration);
    })
    .AddDbContext(builder.Configuration)
    .AddManagementServices()
    .AddCommonServices(builder.Configuration)
    .AddCommandExecutors(builder.Configuration)
    .AddTelegramBotServices();

var app = builder.Build();

await app.Services
    .ConfigureDbContext()
    .InitializeRepository(builder.Configuration, cancellationToken);

app.Run();