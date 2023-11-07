namespace BufferQueue.Concurrency
{
    using Microsoft.Extensions.Logging;
    using System.Collections;
    using System.Collections.Concurrent;

    public abstract class ConcurrentBuffer<T> : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<T> _concurrentQueue;
        private readonly int _maxCapacity;

        public ConcurrentBuffer( ILogger logger, ConcurrentQueue<T> concurrentQueue, int maxCapacity = 100 )
        {
            _logger = logger;
            _concurrentQueue = concurrentQueue;
            _maxCapacity = maxCapacity;
        }

        public virtual void Enqueue( T item )
        {
            if ( item == null || ((ICollection) item).Count == 0 )
            {
                _logger.LogWarning( "Attempted to enqueue an empty or null item." );
                return;
            }

            if ( _concurrentQueue.Count < _maxCapacity )
            {
                _concurrentQueue.Enqueue( item );
            }
            else
            {
                _logger.LogInformation( "Buffer is full, {buffer} is already at {maxCapacity}", nameof( ConcurrentQueue<T> ), _maxCapacity );
            }
        }

        public virtual T? Dequeue( CancellationToken cancellationToken )
        {
            bool dequeueResult = _concurrentQueue.TryDequeue( out T? item );

            if ( !dequeueResult )
            {
                _logger.LogWarning( "Could not successfully dequeue {type}.", typeof( T ).Name );
            }

            return item;
        }

        public virtual int GetBufferSize() => _concurrentQueue.Count;

        public void Dispose() => GC.SuppressFinalize( this );
    }
}