using System.Diagnostics;
using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.BackgroundServices;

public class UsersCleanerJob(
    IUserStateCleanerService userStateCleanerService,
    IServiceScopeFactory scopeFactory,
    IOptions<TelegramBotSettings> options,
    ILogger logger) : BackgroundService
{
    private const string JobTimingMessage = "{0} operating time: {1}ms";

    private readonly Stopwatch _stopwatch = new();
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(options.Value.CleanerJobTimeoutInMinutes), cancellationToken);
            
            _stopwatch.Restart();
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var blackListService = scope.ServiceProvider.GetRequiredService<IBlackListManagementService>();
                
                var clearUsers = userStateCleanerService.ClearOrDeleteAll(cancellationToken);
                var unbanUsers = blackListService.UnbanAllByTimeout(cancellationToken);
                Task.WaitAll([clearUsers, unbanUsers], cancellationToken);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
            }
            finally
            {                
                _stopwatch.Stop();
                logger.Information(string.Format(JobTimingMessage, nameof(UsersCleanerJob), _stopwatch.Elapsed.Milliseconds));
            }
        }
    }
}