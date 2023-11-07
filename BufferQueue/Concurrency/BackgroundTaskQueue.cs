using System.Threading.Channels;

namespace BufferQueue.Concurrency
{
    /// <summary>
    /// Represents a thread-safe task queue that manages asynchronous work items.
    /// </summary>
    public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        /// <summary>
        /// A channel that stores the work items to be processed.
        /// </summary>
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

        /// <summary>
        /// The maximum number of items that the queue can hold.
        /// </summary>
        private readonly int _maxCapacity;

        /// <summary>
        /// Initializes a new instance of the BackgroundTaskQueue class with a specified capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of work items the queue can hold.</param>
        public BackgroundTaskQueue( int capacity )
        {
            _maxCapacity = capacity;
            BoundedChannelOptions options = new( capacity )
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>( options );
        }

        /// <summary>
        /// Asynchronously queues a work item into the task queue.
        /// </summary>
        /// <param name="workItem">The work item to be queued, represented as a function that takes a CancellationToken and returns a ValueTask.</param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided workItem is null.</exception>
        public async ValueTask QueueBackgroundWorkItemAsync( Func<CancellationToken, ValueTask> workItem )
        {
            if ( workItem is null )
            {
                throw new ArgumentNullException( nameof( workItem ) );
            }

            await _queue.Writer.WriteAsync( workItem );
        }

        /// <summary>
        /// Asynchronously dequeues a work item from the task queue.
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for a task to dequeue.</param>
        /// <returns>A function representing the dequeued work item.</returns>
        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync( CancellationToken cancellationToken )
        {
            return await _queue.Reader.ReadAsync( cancellationToken );
        }

        /// <summary>
        /// Retrieves the current number of items in the queue.
        /// </summary>
        /// <returns>The number of items in the queue.</returns>
        public int Count()
        {
            return _queue.Reader.CanCount ? _queue.Reader.Count : 0;
        }

        /// <summary>
        /// Retrieves the maximum capacity of the queue.
        /// </summary>
        /// <returns>The maximum number of items the queue can hold.</returns>
        public int Capacity()
        {
            return _maxCapacity;
        }
    }
}