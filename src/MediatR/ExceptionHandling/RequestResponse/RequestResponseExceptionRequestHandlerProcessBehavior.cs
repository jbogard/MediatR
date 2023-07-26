using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.ExceptionHandling.RequestResponse.Subscription;

namespace MediatR.ExceptionHandling.RequestResponse;

internal sealed class RequestResponseExceptionRequestHandlerProcessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly Type ResponseType = typeof(TResponse);
    private static readonly Type[] RequestResponseTypeHierarchy = MessageTypeResolver.GetMessageTypeHierarchy(typeof(TRequest));

    private readonly IServiceProvider _serviceProvider;

    public RequestResponseExceptionRequestHandlerProcessBehavior(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var state = new RequestResponseExceptionHandlerState<TResponse>();

            foreach (var exceptionType in ExceptionTypeResolver.GetExceptionTypeHierarchy(exception.GetType()))
            {
                foreach (var messageType in RequestResponseTypeHierarchy)
                {
                    if (state.IsHandled)
                    {
                        break;
                    }

                    var handler = ExceptionHandlerFactory.CreateRequestResponseExceptionRequestHandler(messageType, ResponseType, exceptionType);
                    await handler.Handle(request, exception, state, _serviceProvider, cancellationToken);
                }

                if (state.IsHandled)
                {
                    break;
                }
            }

            if (state.IsHandled)
            {
                // The null value must be managed by the user but is also annotated from the response type if it can be null.
                return state.Response!;
            }

            throw;
        }
    }
}