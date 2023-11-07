namespace BufferQueue.Monitors
{
    using BufferQueue.Concurrency;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// An abstract class that manages two types of buffer queues for sending and receiving operations, respectively.
    /// Inherits from the BackgroundService for continuous execution.
    /// </summary>
    /// <typeparam name="TSendType">The type of elements in the send buffer queue.</typeparam>
    /// <typeparam name="TReturnType">The type of elements in the return buffer queue.</typeparam>
    public abstract class DualBufferQueueMonitor<TSendType, TReturnType> : BackgroundService
    {
        /// <summary>
        /// Task queue for managing background tasks.
        /// </summary>
        private readonly IBackgroundTaskQueue _taskQueue;

        /// <summary>
        /// Provider for generating send work items.
        /// </summary>
        private readonly IBufferQueueWorkProvider<TSendType> _sendWorkProvider;

        /// <summary>
        /// Concurrent buffer for storing send work items.
        /// </summary>
        private readonly ConcurrentBuffer<TSendType> _sendBuffer;

        /// <summary>
        /// Provider for generating return work items.
        /// </summary>
        private readonly IBufferQueueWorkProvider<TReturnType> _returnWorkProvider;

        /// <summary>
        /// Concurrent buffer for storing return work items.
        /// </summary>
        private readonly ConcurrentBuffer<TReturnType> _returnBuffer;

        /// <summary>
        /// Logger instance for logging messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Cancellation token that signals the stopping of the application.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the DualBufferQueueMonitor class.
        /// </summary>
        /// <param name="logger">The logger instance used for logging.</param>
        /// <param name="taskQueue">The task queue used for queuing background work items.</param>
        /// <param name="sendWorkProvider">Provider for generating send work items.</param>
        /// <param name="sendBuffer">The concurrent buffer to store send work items.</param>
        /// <param name="returnWorkProvider">Provider for generating return work items.</param>
        /// <param name="returnBuffer">The concurrent buffer to store return work items.</param>
        /// <param name="applicationLifetime">Application lifetime for managing application events.</param>
        public DualBufferQueueMonitor
            (
                ILogger logger,
                IBackgroundTaskQueue taskQueue,
                IBufferQueueWorkProvider<TSendType> sendWorkProvider,
                ConcurrentBuffer<TSendType> sendBuffer,
                IBufferQueueWorkProvider<TReturnType> returnWorkProvider,
                ConcurrentBuffer<TReturnType> returnBuffer,
                IHostApplicationLifetime applicationLifetime
            )
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _sendWorkProvider = sendWorkProvider;
            _sendBuffer = sendBuffer;
            _returnWorkProvider = returnWorkProvider;
            _returnBuffer = returnBuffer;
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
        /// Monitors and manages the enqueuing of work items into the send and return buffer queues.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        private async ValueTask MonitorAsync()
        {
            while ( !_cancellationToken.IsCancellationRequested )
            {
                var canQueueReturnWork = _returnBuffer.Count() < _returnBuffer.Capacity();
                var canQueueSendWork = _sendBuffer.Count() < _sendBuffer.Capacity();
                var taskQueueNotFull = _taskQueue.Count() < _taskQueue.Capacity();

                if ( canQueueReturnWork && taskQueueNotFull )
                {
                    await _taskQueue.QueueBackgroundWorkItemAsync( CreateReturnWorkItem );
                }
                else if ( canQueueSendWork && taskQueueNotFull )
                {
                    await _taskQueue.QueueBackgroundWorkItemAsync( CreateSendWorkItem );
                }
                else
                {
                    _logger.LogDebug( "Both buffers are full, not able to enqueue more work items." );
                }
            }
        }

        /// <summary>
        /// Creates a work item for the return buffer and enqueues it.
        /// </summary>
        /// <param name="token">Cancellation token for handling task cancellation.</param>
        /// <returns>A ValueTask representing the asynchronous operation of creating a return work item.</returns>
        private async ValueTask CreateReturnWorkItem( CancellationToken token )
        {
            while ( !token.IsCancellationRequested )
            {
                try
                {
                    _returnBuffer.Enqueue( await _returnWorkProvider.Enqueue() );
                }
                catch ( OperationCanceledException )
                {
                    // Prevent throwing if the operation is cancelled
                }
            }
        }

        /// <summary>
        /// Creates a work item for the send buffer and enqueues it.
        /// </summary>
        /// <param name="token">Cancellation token for handling task cancellation.</param>
        /// <returns>A ValueTask representing the asynchronous operation of creating a send work item.</returns>
        private async ValueTask CreateSendWorkItem( CancellationToken token )
        {
            while ( !token.IsCancellationRequested )
            {
                try
                {
                    _sendBuffer.Enqueue( await _sendWorkProvider.Enqueue() );
                }
                catch ( OperationCanceledException )
                {
                    // Prevent throwing if the operation is cancelled
                }
            }
        }
    }

}
