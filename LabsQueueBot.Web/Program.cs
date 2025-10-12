using LabsQueueBot.BusinessLogic;
using LabsQueueBot.Repository;
using LabsQueueBot.Web.ServiceCollectionExtensions;
using Serilog;

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var builder = Host.CreateApplicationBuilder();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

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
    .AddTelegramBotServices(builder.Configuration);

var app = builder.Build();

await app.Services
    .ConfigureDbContext()
    .InitializeRepository(cancellationToken);

app.Run();