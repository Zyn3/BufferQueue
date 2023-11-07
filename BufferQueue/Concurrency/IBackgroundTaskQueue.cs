namespace BufferQueue.Concurrency
{
    /// <summary>
    /// Defines an interface for a background task queue that can queue and dequeue work items asynchronously.
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// Asynchronously queues a work item into the task queue.
        /// </summary>
        /// <param name="workItem">The work item to be queued, represented as a function that takes a CancellationToken and returns a ValueTask.</param>
        /// <returns>A ValueTask representing the asynchronous operation of enqueuing the work item.</returns>
        ValueTask QueueBackgroundWorkItemAsync(
            Func<CancellationToken, ValueTask> workItem );

        /// <summary>
        /// Asynchronously dequeues a work item from the task queue.
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for a task to dequeue.</param>
        /// <returns>A ValueTask that results in a function representing the dequeued work item.</returns>
        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken );

        /// <summary>
        /// Retrieves the current number of items in the queue.
        /// </summary>
        /// <returns>The number of items currently in the queue.</returns>
        int Count();

        /// <summary>
        /// Retrieves the maximum capacity of the queue.
        /// </summary>
        /// <returns>The maximum number of items that the queue can hold.</returns>
        int Capacity();
    }
}