using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.ExceptionHandling.Request.Subscription;

namespace MediatR.ExceptionHandling.Request;

internal sealed class RequestExceptionHandlerProcessBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private static readonly Type[] RequestTypeHierarchy = MessageTypeResolver.GetMessageTypeHierarchy(typeof(TRequest));

    private readonly IServiceProvider _serviceProvider;

    public RequestExceptionHandlerProcessBehavior(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        try
        {
            await next(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var state = new RequestExceptionHandlerState();
            foreach (var exceptionType in ExceptionTypeResolver.GetExceptionTypeHierarchy(exception.GetType()))
            {
                foreach (var messageType in RequestTypeHierarchy)
                {
                    if (state.IsHandled)
                    {
                        break;
                    }

                    var handler = ExceptionHandlerFactory.CreateRequestExceptionRequestHandler(messageType, exceptionType);
                    await handler.HandleAsync(request, exception, state, _serviceProvider, cancellationToken);
                }

                if (state.IsHandled)
                {
                    break;
                }
            }

            if (!state.IsHandled)
            {
                throw;
            }
        }
    }
}