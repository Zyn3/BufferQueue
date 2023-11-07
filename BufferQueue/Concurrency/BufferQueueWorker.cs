namespace BufferQueue.Concurrency
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class BufferQueueWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public BufferQueueWorker( ILogger logger, IBackgroundTaskQueue taskQueue )
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {

            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    var shouldProcess = await _taskQueue.Count() > 0;

                    if ( shouldProcess )
                    {
                        await _taskQueue.DequeueAsync( stoppingToken );
                    }
                    else
                    {
                        await Task.Delay( 100, stoppingToken );
                    }
                }
                catch ( OperationCanceledException )
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "Error occurred executing task work item." );
                }
            }
        }

        public override async Task StopAsync( CancellationToken stoppingToken )
        {
            await base.StopAsync( stoppingToken );
        }
    }
}
