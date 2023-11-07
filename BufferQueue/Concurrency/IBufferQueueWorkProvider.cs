namespace BufferQueue.Concurrency
{
    /// <summary>
    /// Defines an interface for providing elements of type <typeparamref name="TBufferType"/> to be enqueued in a buffer queue.
    /// </summary>
    /// <typeparam name="TBufferType">The type of elements to be provided for enqueueing.</typeparam>
    public interface IBufferQueueWorkProvider<TBufferType>
    {
        /// <summary>
        /// Asynchronously provides an element to be enqueued in the buffer queue.
        /// </summary>
        /// <returns>A ValueTask that results in the element of type <typeparamref name="TBufferType"/> to be enqueued.</returns>
        ValueTask<TBufferType> Enqueue();
    }
}
