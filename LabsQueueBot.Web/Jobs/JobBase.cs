using System.Diagnostics;
using ILogger = Serilog.ILogger;

namespace LabsQueueBot.Web.Jobs;

public abstract class JobBase(
    ILogger logger) : BackgroundService
{
    private const string JobFailMessage = "{0} has failed with exception:";
    private const string JobProfilingMessage = "{0} operating time: {1}ms, {2}";
    private const string SuccessResult = "success";
    private const string FailResult = "fail";
    
    private readonly Stopwatch _stopwatch = new();
    
    protected abstract TimeSpan JobDelayBeforeStart { get; }
    protected abstract TimeSpan JobTimeout { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(JobDelayBeforeStart, cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            var isSuccess = true;
            try
            {
                _stopwatch.Restart();
                await JobBody(cancellationToken);
            }
            catch (Exception e)
            {
                isSuccess = false;
                logger.Error(e, string.Format(JobFailMessage, GetType().Name));
            }
            finally
            {
                _stopwatch.Stop();
                logger.Information(string.Format(JobProfilingMessage,
                    GetType().Name, _stopwatch.Elapsed.Milliseconds, isSuccess ? SuccessResult : FailResult));
            }
            await Task.Delay(JobTimeout, cancellationToken);
        }
    }

    protected abstract Task JobBody(CancellationToken cancellationToken);
}