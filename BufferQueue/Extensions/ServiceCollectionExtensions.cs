namespace BufferQueue.Extensions
{
    using BufferQueue.Concurrency;
    using BufferQueue.Monitors;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupBufferQueue( this IServiceCollection services, int maxQueueSize )
        {
            services.AddSingleton<ConcurrentBuffer<string[]>>();
            services.AddSingleton<IBackgroundTaskQueue>( _ =>
            {
                return new BackgroundTaskQueue( maxQueueSize );
            } );

            services.AddHostedService<BufferQueueWorker>();
            services.AddHostedService<BufferQueueMonitor<string[]>>();

            return services;
        }

        private static IServiceCollection SetupDualBufferQueue( this IServiceCollection services, int maxQueueSize )
        {
            services.AddSingleton<ConcurrentBuffer<string[]>>();
            services.AddSingleton<ConcurrentBuffer<Task<List<int>>>>();
            services.AddSingleton<IBackgroundTaskQueue>( _ =>
            {
                return new BackgroundTaskQueue( maxQueueSize );
            } );

            services.AddHostedService<BufferQueueWorker>();
            services.AddHostedService<DualBufferQueueMonitor<string[], Task<List<int>>>>();


            return services;
        }
    }
}
