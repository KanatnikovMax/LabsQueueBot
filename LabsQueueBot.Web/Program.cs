using LabsQueueBot.BusinessLogic;
using LabsQueueBot.Repository;
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
    .AddPersistence()
    .AddManagementServices()
    .AddCommonServices(builder.Configuration)
    .AddCommandExecutors()
    .AddTelegramBotServices();

var app = builder.Build();

await app.Services
    .ConfigureDbContext()
    .InitializeRepository(builder.Configuration, cancellationToken);

app.Run();