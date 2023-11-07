namespace BufferQueue.Monitors
{
    using BufferQueue.Concurrency;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class BufferQueueMonitor<TBuffer> : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ConcurrentBuffer<TBuffer> _buffer;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        private const int MaxBufferSize = 100;

        public BufferQueueMonitor
            (
                ILogger logger,
                IBackgroundTaskQueue taskQueue,
                ConcurrentBuffer<TBuffer> buffer,
                IHostApplicationLifetime applicationLifetime
            )
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _buffer = buffer;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        protected override Task ExecuteAsync( CancellationToken stoppingToken )
        {
            return MonitorAsync().AsTask();
        }

        private async ValueTask MonitorAsync()
        {
            while ( !_cancellationToken.IsCancellationRequested )
            {
                var bufferSize = _buffer.GetBufferSize();

                if ( bufferSize < MaxBufferSize )
                {
                    await _taskQueue.QueueBackgroundWorkItemAsync( BuildWorkItemAsync<TBuffer> );
                }
                else
                {
                    _logger.LogDebug( "Buffer is full, not able to enqueue more work items." );
                }
            }
        }

        protected abstract ValueTask BuildWorkItemAsync<T>( CancellationToken token );
    }
}
