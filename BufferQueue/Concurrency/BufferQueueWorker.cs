namespace BufferQueue.Concurrency
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents a background service that continuously processes work items from a task queue.
    /// </summary>
    public class BufferQueueWorker : BackgroundService
    {
        /// <summary>
        /// Logger instance for logging messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The task queue from which work items are to be dequeued and processed.
        /// </summary>
        private readonly IBackgroundTaskQueue _taskQueue;

        /// <summary>
        /// Initializes a new instance of the BufferQueueWorker class with the specified logger and task queue.
        /// </summary>
        /// <param name="logger">The logger instance used for logging.</param>
        /// <param name="taskQueue">The task queue to be used for dequeuing and processing work items.</param>
        public BufferQueueWorker( ILogger logger, IBackgroundTaskQueue taskQueue )
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        /// <summary>
        /// Asynchronously executes the background service, processing work items from the task queue.
        /// </summary>
        /// <param name="stoppingToken">A CancellationToken that signals the stop of the background service.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    var shouldProcess = _taskQueue.Count() > 0;

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

        /// <summary>
        /// Asynchronously stops the background service.
        /// </summary>
        /// <param name="stoppingToken">A CancellationToken that signals the stop of the background service.</param>
        /// <returns>A Task representing the asynchronous stop operation.</returns>
        public override async Task StopAsync( CancellationToken stoppingToken )
        {
            await base.StopAsync( stoppingToken );
        }
    }
}
