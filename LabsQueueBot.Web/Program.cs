using LabsQueueBot.BusinessLogic;
using LabsQueueBot.Core.Settings;
using LabsQueueBot.Web.ServiceCollectionExtensions;
using LabsQueueBot.Web.SettingsReader;
using Serilog;

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var builder = Host.CreateApplicationBuilder();

var queueBotSettings = QueueBotSettingsReader.Read(builder.Configuration);
var commandsSettings = CommandsSettingsReader.Read(builder.Configuration);

builder.Services.Configure<QueueBotSettings>(builder.Configuration.GetRequiredSection("LabsQueueBot"));

builder.Services.AddSerilog(loggerConfiguration =>
{
    loggerConfiguration
        .Enrich.WithCorrelationId()
        .ReadFrom.Configuration(builder.Configuration);
});
builder.Services.AddDbContext(queueBotSettings);
builder.Services.AddUserManageServices(queueBotSettings);
builder.Services.AddServices(queueBotSettings);
builder.Services.AddCommands(queueBotSettings, commandsSettings);
builder.Services.AddTelegramBotServices(queueBotSettings, cancellationToken);

var app = builder.Build();

app.Services.ConfigureDbContext();
await app.Services.InitializeRepository(queueBotSettings, cancellationToken);

app.Run();