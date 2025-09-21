using System.Diagnostics;
using LabsQueueBot.BusinessLogic.Services;
using LabsQueueBot.Core.Settings;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.BackgroundServices;

public class UsersCleanerJob(
    IUserCleanerService userCleanerService,
    QueueBotSettings settings,
    ILogger logger) : BackgroundService
{
    private const string JobTimingMessage = "Job operating time: {0}ms";
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // await Task.Delay(TimeSpan.FromMinutes(settings.UserStateUpdateTimeoutInMinutes), cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await userCleanerService.ClearOrDeleteAll(cancellationToken);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
            }
            finally
            {
                stopwatch.Stop();
                logger.Information(string.Format(JobTimingMessage, stopwatch.Elapsed.Milliseconds));
            }
        }
    }
}