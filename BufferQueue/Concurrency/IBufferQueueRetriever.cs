namespace BufferQueue.Concurrency
{
    /// <summary>
    /// Defines an interface for retrieving elements of type <typeparamref name="TBufferType"/> from a buffer queue.
    /// </summary>
    /// <typeparam name="TBufferType">The type of elements in the buffer queue.</typeparam>
    public interface IBufferQueueRetriever<TBufferType>
    {
        /// <summary>
        /// Asynchronously dequeues an element from the buffer queue.
        /// </summary>
        /// <returns>A ValueTask that results in the dequeued element of type <typeparamref name="TBufferType"/>.</returns>
        ValueTask<TBufferType> Dequeue();
    }
}
