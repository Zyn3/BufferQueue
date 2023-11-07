namespace BufferQueue.Extensions
{
    using BufferQueue.Concurrency;
    using BufferQueue.Monitors;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    /// <summary>
    /// Provides extension methods for setting up buffer queue services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures services for a single buffer queue system in the service collection.
        /// </summary>
        /// <typeparam name="TBufferType">The type of elements in the buffer queue.</typeparam>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="bufferWorkProviderImplementation">The implementation of the buffer queue work provider.</param>
        /// <param name="maxQueueSize">The maximum size of the task queue. Defaults to 100.</param>
        /// <returns>The updated IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services or bufferWorkProviderImplementation is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if IBufferQueueRetriever<TBufferType> is not registered in the service collection.</exception>
        public static IServiceCollection SetupBufferQueue<TBufferType>(
            this IServiceCollection services,
            IBufferQueueWorkProvider<TBufferType> bufferWorkProviderImplementation,
            int maxQueueSize = 100 )
        {
            if ( services is null )
            {
                throw new ArgumentNullException( nameof( services ) );
            }

            if ( bufferWorkProviderImplementation is null )
            {
                throw new ArgumentNullException( nameof( bufferWorkProviderImplementation ) );
            }

            if ( !services.Select( x => x.ServiceType == typeof( IBufferQueueRetriever<TBufferType> ) ).Any() )
            {
                throw new InvalidOperationException( $"No registration found for type {nameof( IBufferQueueRetriever<TBufferType> )} in the service collection." );
            }

            services.AddSingleton<ConcurrentBuffer<TBufferType>>();
            services.AddSingleton<IBackgroundTaskQueue>( _ =>
            {
                return new BackgroundTaskQueue( maxQueueSize );
            } );
            services.AddSingleton( sp => bufferWorkProviderImplementation );

            services.AddHostedService<BufferQueueWorker>();
            services.AddHostedService<BufferQueueMonitor<TBufferType>>();

            return services;
        }

        /// <summary>
        /// Configures services for a dual buffer queue system (for both sending and returning types) in the service collection.
        /// </summary>
        /// <typeparam name="TSendType">The type of elements in the send buffer queue.</typeparam>
        /// <typeparam name="TReturnType">The type of elements in the return buffer queue.</typeparam>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="sendBufferWorkProviderImplementation">The implementation of the send buffer work provider.</param>
        /// <param name="returnBufferWorkProviderImplementation">The implementation of the return buffer work provider.</param>
        /// <param name="maxQueueSize">The maximum size of the task queue. Defaults to 100.</param>
        /// <returns>The updated IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services, sendBufferWorkProviderImplementation, or returnBufferWorkProviderImplementation is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if IBufferQueueRetriever<TSendType> or IBufferQueueRetriever<TReturnType> is not registered in the service collection.</exception>
        public static IServiceCollection SetupDualBufferQueue<TSendType, TReturnType>(
            this IServiceCollection services,
            IBufferQueueWorkProvider<TSendType> sendBufferWorkProviderImplementation,
            IBufferQueueWorkProvider<TReturnType> returnBufferWorkProviderImplementation,
            int maxQueueSize = 100 )
        {
            if ( services is null )
            {
                throw new ArgumentNullException( nameof( services ) );
            }

            if ( sendBufferWorkProviderImplementation is null )
            {
                throw new ArgumentNullException( nameof( sendBufferWorkProviderImplementation ) );
            }

            if ( returnBufferWorkProviderImplementation is null )
            {
                throw new ArgumentNullException( nameof( returnBufferWorkProviderImplementation ) );
            }

            if ( !services.Select( x => x.ServiceType == typeof( IBufferQueueRetriever<TSendType> ) ).Any() )
            {
                throw new InvalidOperationException( $"No registration found for type {nameof( IBufferQueueRetriever<TSendType> )} in the service collection." );
            }

            if ( !services.Select( x => x.ServiceType == typeof( IBufferQueueRetriever<TReturnType> ) ).Any() )
            {
                throw new InvalidOperationException( $"No registration found for type {nameof( IBufferQueueRetriever<TReturnType> )} in the service collection." );
            }

            services.AddSingleton<ConcurrentBuffer<TSendType>>();
            services.AddSingleton<ConcurrentBuffer<TReturnType>>();
            services.AddSingleton<IBackgroundTaskQueue>( _ =>
            {
                return new BackgroundTaskQueue( maxQueueSize );
            } );
            services.AddSingleton( sp => sendBufferWorkProviderImplementation );
            services.AddSingleton( sp => returnBufferWorkProviderImplementation );

            services.AddHostedService<BufferQueueWorker>();
            services.AddHostedService<DualBufferQueueMonitor<TSendType, TReturnType>>();

            return services;
        }
    }
}
