using System.Threading;
using System.Threading.Tasks;

namespace MediatR
{
    public static class PipelineExtensions
    {
        public static Task<TResponse> Send<TResponse>(this IRequest<TResponse> request, IMediator mediator,
            CancellationToken cancellationToken = default)
            => mediator.Send(request, cancellationToken);

        public static Task<TResponse> Send<TRequest, TResponse>(this TRequest request,
            IRequestHandler<TRequest, TResponse> handler, CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>
            => handler.Handle(request, cancellationToken);

        public static Task Publish(this INotification notification, IMediator mediator,
            CancellationToken cancellationToken = default)
            => mediator.Publish(notification, cancellationToken);

        public static Task Publish<TNotification>(this TNotification notification,
            INotificationHandler<TNotification> handler, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => handler.Handle(notification, cancellationToken);
    }
}
