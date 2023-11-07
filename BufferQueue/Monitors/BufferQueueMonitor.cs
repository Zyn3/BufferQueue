using BufferQueue.Concurrency;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BufferQueue.Monitors
{
    /// <summary>
    /// An abstract class that serves as a monitor for a buffer queue, managing the enqueueing of work items.
    /// It inherits from the BackgroundService for continuous execution.
    /// </summary>
    /// <typeparam name="TBufferType">The type of elements in the buffer queue.</typeparam>
    public abstract class BufferQueueMonitor<TBufferType> : BackgroundService
    {
        /// <summary>
        /// Provider for buffer queue work items.
        /// </summary>
        private readonly IBufferQueueWorkProvider<TBufferType> _workProvider;

        /// <summary>
        /// Task queue for managing background tasks.
        /// </summary>
        private readonly IBackgroundTaskQueue _taskQueue;

        /// <summary>
        /// Concurrent buffer for storing work items.
        /// </summary>
        private readonly ConcurrentBuffer<TBufferType> _buffer;

        /// <summary>
        /// Logger instance for logging messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Cancellation token that signals the stopping of the application.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the BufferQueueMonitor class.
        /// </summary>
        /// <param name="logger">The logger instance used for logging.</param>
        /// <param name="taskQueue">The task queue used for queuing background work items.</param>
        /// <param name="buffer">The concurrent buffer to store work items.</param>
        /// <param name="workProvider">Provider for generating work items for the buffer.</param>
        /// <param name="applicationLifetime">Application lifetime for managing application events.</param>
        public BufferQueueMonitor
            (
                ILogger logger,
                IBackgroundTaskQueue taskQueue,
                ConcurrentBuffer<TBufferType> buffer,
                IBufferQueueWorkProvider<TBufferType> workProvider,
                IHostApplicationLifetime applicationLifetime
            )
        {
            _workProvider = workProvider;
            _taskQueue = taskQueue;
            _logger = logger;
            _buffer = buffer;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        /// <summary>
        /// Executes the background service logic.
        /// </summary>
        /// <param name="stoppingToken">A CancellationToken that signals the stop of the background service.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override Task ExecuteAsync( CancellationToken stoppingToken )
        {
            return MonitorAsync().AsTask();
        }

        /// <summary>
        /// Monitors and manages the enqueuing of work items into the buffer queue.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        private async ValueTask MonitorAsync()
        {
            while ( !_cancellationToken.IsCancellationRequested )
            {
                var canQueueBufferWork = _buffer.Count() < _buffer.Capacity();
                var taskQueueNotFull = _taskQueue.Count() < _taskQueue.Capacity();

                if ( canQueueBufferWork && taskQueueNotFull )
                {
                    await _taskQueue.QueueBackgroundWorkItemAsync( CreateWorkItem );
                }
                else
                {
                    _logger.LogDebug( "Buffer is full, not able to enqueue more work items." );
                }
            }
        }

        /// <summary>
        /// Creates a work item to be enqueued in the buffer.
        /// </summary>
        /// <param name="token">Cancellation token for handling task cancellation.</param>
        /// <returns>A ValueTask representing the asynchronous operation of creating a work item.</returns>
        private async ValueTask CreateWorkItem( CancellationToken token )
        {
            while ( !token.IsCancellationRequested )
            {
                try
                {
                    _buffer.Enqueue( await _workProvider.Enqueue() );
                }
                catch ( OperationCanceledException )
                {
                    // Prevent throwing if the operation is cancelled
                }
            }
        }
    }
}
