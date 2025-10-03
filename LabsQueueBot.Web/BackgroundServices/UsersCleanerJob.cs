using System.Diagnostics;
using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Settings;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.BackgroundServices;

public class UsersCleanerJob(
    IUserStateCleanerService userStateCleanerService,
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
                await userStateCleanerService.ClearOrDeleteAll(cancellationToken);
                // TODO UsersCleanerJob должна разбанить пользователей, если время бана истекло
                // await userUnbanService.UnbanAllByTiemout(cancellationToken);
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