namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class NotificationHandler
    {
        public abstract Task Handle(INotification notification, CancellationToken cancellationToken, IMediatorContext context, MultiInstanceFactory multiInstanceFactory, Func<IEnumerable<RequestHandlerDelegate<Unit>>, Task> publish);
    }

    internal class NotificationHandlerImpl<TNotification> : NotificationHandler
        where TNotification : INotification
    {
        public override Task Handle(INotification notification, CancellationToken cancellationToken, IMediatorContext context, MultiInstanceFactory multiInstanceFactory, Func<IEnumerable<RequestHandlerDelegate<Unit>>, Task> publish)
        {
            var handlers = GetHandlers((TNotification)notification, cancellationToken, context, multiInstanceFactory);
            var pipeline = GetPipeline((TNotification)notification, context, handlers, multiInstanceFactory, publish);

            return pipeline;
        }

        private static async Task GetPipeline(TNotification request, IMediatorContext context, IEnumerable<RequestHandlerDelegate<Unit>> handlers, MultiInstanceFactory factory, Func<IEnumerable<RequestHandlerDelegate<Unit>>, Task> publish)
        {
            var behaviors = factory(typeof(IPipelineBehavior<TNotification, Unit>))
                .Cast<IPipelineBehavior<TNotification, Unit>>()
                .Reverse()
                .ToArray();

            var tasks = new List<RequestHandlerDelegate<Unit>>();
            foreach (var handle in handlers)
            {
                RequestHandlerDelegate<Unit> handleCoreDelegate = async () =>
                {
                    await handle();
                    return Unit.Value;
                };
                var aggregate = behaviors.Aggregate(handleCoreDelegate, (next, pipeline) => () => pipeline.Handle(request, next, context));
                tasks.Add(aggregate);
            }

            await publish(tasks);
        }

        private static IEnumerable<THandler> GetHandlers<THandler>(MultiInstanceFactory factory)
        {
            return factory(typeof(THandler)).Cast<THandler>();
        }

        private IEnumerable<RequestHandlerDelegate<Unit>> GetHandlers(TNotification notification, CancellationToken cancellationToken, IMediatorContext context, MultiInstanceFactory factory)
        {
            var notificationHandlers = GetHandlers<INotificationHandler<TNotification>>(factory)
                .Select(x =>
                    {
                        return new RequestHandlerDelegate<Unit>(() =>
                        {
                            x.Handle(notification);
                            return Task.FromResult(Unit.Value);
                        });
                    });

            var asyncNotificationHandlers = GetHandlers<IAsyncNotificationHandler<TNotification>>(factory)
                .Select(x =>
               {
                   return new RequestHandlerDelegate<Unit>(async () =>
                   {
                       await x.Handle(notification).ConfigureAwait(false);
                       return Unit.Value;
                   });
               });

            var cancellableAsyncNotificationHandlers = GetHandlers<ICancellableAsyncNotificationHandler<TNotification>>(factory)
                    .Select( x =>
                   {
                       return new RequestHandlerDelegate<Unit>(async () =>
                       {
                           await x.Handle(notification, cancellationToken).ConfigureAwait(false);
                           return Unit.Value;
                       });
                   });

            var contextualAsyncNotificationHandlers = GetHandlers<IContextualAsyncNotificationHandler<TNotification>>(factory)
                .Select( x =>
                {
                    return new RequestHandlerDelegate<Unit>(async () =>
                    {
                        await x.Handle(notification, context).ConfigureAwait(false);
                        return Unit.Value;
                    });
                });

            var allHandlers = notificationHandlers
                .Concat(asyncNotificationHandlers)
                .Concat(cancellableAsyncNotificationHandlers)
                .Concat(contextualAsyncNotificationHandlers);

            return allHandlers;
        }

    }
}