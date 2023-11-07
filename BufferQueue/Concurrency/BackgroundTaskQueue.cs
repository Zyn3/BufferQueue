using System.Threading.Channels;

namespace BufferQueue.Concurrency
{
    public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

        public BackgroundTaskQueue( int capacity )
        {
            BoundedChannelOptions options = new( capacity )
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>( options );
        }

        public async ValueTask QueueBackgroundWorkItemAsync( Func<CancellationToken, ValueTask> workItem )
        {
            if ( workItem is null )
            {
                throw new ArgumentNullException( nameof( workItem ) );
            }

            await _queue.Writer.WriteAsync( workItem );
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync( CancellationToken cancellationToken )
        {
            return await _queue.Reader.ReadAsync( cancellationToken );
        }

        public async Task<int> Count()
        {
            return await Task.FromResult( _queue.Reader.CanCount ? _queue.Reader.Count : 0 );
        }
    }
}