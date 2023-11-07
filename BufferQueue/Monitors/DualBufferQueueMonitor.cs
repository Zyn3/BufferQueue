namespace BufferQueue.Monitors
{
    using BufferQueue.Concurrency;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class DualBufferQueueMonitor<TSendType, TReturnType> : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ConcurrentBuffer<TSendType> _sendBuffer;
        private readonly ConcurrentBuffer<TReturnType> _returnBuffer;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        private const int MaxReturnBufferSize = 100;
        private const int MaxSendBufferSize = 100;

        public DualBufferQueueMonitor
            (
                ILogger logger,
                IBackgroundTaskQueue taskQueue,
                ConcurrentBuffer<TSendType> sendBuffer,
                ConcurrentBuffer<TReturnType> returnBuffer,
                IHostApplicationLifetime applicationLifetime
            )
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _sendBuffer = sendBuffer;
            _returnBuffer = returnBuffer;
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
                var returnBufferSize = _returnBuffer.GetBufferSize();
                var sendBufferSize = _sendBuffer.GetBufferSize();

                if ( returnBufferSize < MaxReturnBufferSize )
                {
                    await _taskQueue.QueueBackgroundWorkItemAsync( CreateReturnBufferWork<TReturnType> );
                }
                else if ( sendBufferSize < MaxSendBufferSize )
                {
                    await _taskQueue.QueueBackgroundWorkItemAsync( CreateSendBufferWork<TSendType> );
                }
                else
                {
                    _logger.LogDebug( "Both buffers are full, not able to enqueue more work items." );
                }
            }
        }

        protected abstract ValueTask CreateSendBufferWork<TSendType>( CancellationToken token );
        protected abstract ValueTask CreateReturnBufferWork<TReturnType>( CancellationToken token );
    }
}
