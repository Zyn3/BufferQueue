namespace BufferQueue.Concurrency
{
    using Microsoft.Extensions.Logging;
    using System.Collections;
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents a thread-safe buffer that handles concurrent operations on a queue of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the buffer.</typeparam>
    public abstract class ConcurrentBuffer<T> : IDisposable
    {
        /// <summary>
        /// Logger instance for logging messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The concurrent queue that stores items.
        /// </summary>
        private readonly ConcurrentQueue<T> _concurrentQueue;

        /// <summary>
        /// The maximum number of items that the buffer can hold.
        /// </summary>
        private readonly int _maxCapacity;

        /// <summary>
        /// Initializes a new instance of the ConcurrentBuffer class with the specified logger, concurrent queue, and maximum capacity.
        /// </summary>
        /// <param name="logger">The logger instance used for logging.</param>
        /// <param name="concurrentQueue">The concurrent queue to store items.</param>
        /// <param name="maxCapacity">The maximum number of items the buffer can hold. Default is 100.</param>
        public ConcurrentBuffer( ILogger logger, ConcurrentQueue<T> concurrentQueue, int maxCapacity = 100 )
        {
            _logger = logger;
            _concurrentQueue = concurrentQueue;
            _maxCapacity = maxCapacity;
        }

        /// <summary>
        /// Enqueues an item to the buffer. Logs a warning if the item is null or empty, and logs information if the buffer is full.
        /// </summary>
        /// <param name="item">The item to be enqueued to the buffer.</param>
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

        /// <summary>
        /// Attempts to dequeue an item from the buffer. Logs a warning if unable to dequeue.
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the dequeue operation.</param>
        /// <returns>The dequeued item if successful; otherwise, default value of <typeparamref name="T"/>.</returns>
        public virtual T? Dequeue( CancellationToken cancellationToken )
        {
            bool dequeueResult = _concurrentQueue.TryDequeue( out T? item );

            if ( !dequeueResult )
            {
                _logger.LogWarning( "Could not successfully dequeue {type}.", typeof( T ).Name );
            }

            return item;
        }

        /// <summary>
        /// Releases all resources used by the ConcurrentBuffer.
        /// </summary>
        public void Dispose() => GC.SuppressFinalize( this );

        /// <summary>
        /// Gets the current count of items in the buffer.
        /// </summary>
        /// <returns>The number of items in the buffer.</returns>
        internal int Count() => _concurrentQueue.Count;

        /// <summary>
        /// Gets the maximum capacity of the buffer.
        /// </summary>
        /// <returns>The maximum number of items the buffer can hold.</returns>
        internal int Capacity() => _maxCapacity;
    }
}