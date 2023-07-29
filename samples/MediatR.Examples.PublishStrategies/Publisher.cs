using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;
using MediatR.Subscriptions.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.PublishStrategies;

public class Publisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<PublishStrategy, INotificationPublisher> _publishStrategies = new();

    public Publisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _publishStrategies[PublishStrategy.Async] = new AsyncContinueOnException();
        _publishStrategies[PublishStrategy.ParallelNoWait] = new ParallelNoWait();
        _publishStrategies[PublishStrategy.ParallelWhenAll] = new ParallelWhenAll();
        _publishStrategies[PublishStrategy.ParallelWhenAny] = new ParallelWhenAny();
        _publishStrategies[PublishStrategy.SyncContinueOnException] = new SyncContinueOnException();
        _publishStrategies[PublishStrategy.SyncStopOnException] = new SyncStopOnException();
    }

    public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        if (!_publishStrategies.TryGetValue(strategy, out var publisher))
        {
            throw new ArgumentException($"Unknown strategy: {strategy}");
        }

        return publisher.Publish(_serviceProvider.GetServices<INotificationHandler<TNotification>>().ToArray(), notification, cancellationToken);
    }
    
    private sealed class ParallelWhenAll : INotificationPublisher
    {
        public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
            where TNotification : INotification =>
            notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        public Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            var tasks = new Task[notificationHandlers.Length];

            for (var i = 0; i < notificationHandlers.Length; i++)
            {
                var notificationHandler = notificationHandlers[i];
                tasks[i] = notificationHandler.Handle(notification, cancellationToken);
            }

            return Task.WhenAll(tasks);
        }
    }
    
    private sealed class ParallelWhenAny : INotificationPublisher
    {
        public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
            where TNotification : INotification =>
            notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        public Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            var tasks = new Task[notificationHandlers.Length];

            for (var i = 0; i < notificationHandlers.Length; i++)
            {
                var notificationHandler = notificationHandlers[i];
                tasks[i] = Task.Run(() => notificationHandler.Handle(notification, cancellationToken), cancellationToken);
            }

            return Task.WhenAny(tasks);
        }
    }
    
    private sealed class ParallelNoWait : INotificationPublisher
    {
        public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
            where TNotification : INotification =>
            notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        public Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            foreach (var notificationHandler in notificationHandlers)
            {
                notificationHandler.Handle(notification, cancellationToken);
            }
            
            return Task.CompletedTask;
        }
    }
    
    private sealed class AsyncContinueOnException : INotificationPublisher
    {
        public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
            where TNotification : INotification =>
            notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            var tasks = new Task[notificationHandlers.Length];
            var exceptions = new List<Exception>();

            for (var i = 0; i < notificationHandlers.Length; i++)
            {
                try
                {
                    tasks[i] = notificationHandlers[i].Handle(notification, cancellationToken);
                }
                catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
            {
                exceptions.Add(ex);
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
    
    private sealed class SyncStopOnException : INotificationPublisher
    {
        public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
            where TNotification : INotification =>
            notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            foreach (var notificationHandler in notificationHandlers)
            {
                await notificationHandler.Handle(notification, cancellationToken);
            }
        }
    }
    
    private sealed class SyncContinueOnException : INotificationPublisher
    {
        public void Publish<TNotification>(NotificationHandler notificationHandler, TNotification notification, IServiceProvider serviceProvider, INotificationPublisher notificationPublisher, CancellationToken cancellationToken)
            where TNotification : INotification =>
            notificationHandler.Handle(notification, serviceProvider, notificationPublisher, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task Publish<TNotification>(INotificationHandler<TNotification>[] notificationHandlers, TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            var exceptions = new List<Exception>();

            foreach (var handler in notificationHandlers)
            {
                try
                {
                    await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
                }
                catch (AggregateException ex)
                {
                    exceptions.AddRange(ex.Flatten().InnerExceptions);
                }
                catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}