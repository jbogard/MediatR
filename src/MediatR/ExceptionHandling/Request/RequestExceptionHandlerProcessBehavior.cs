using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExceptionHandling;

internal sealed class RequestExceptionHandlerProcessBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private static readonly Type[] RequestTypeHierarchy = MessageTypeResolver.MessageTypeHierarchyFactory(typeof(TRequest));

    private readonly ExceptionHandlerFactory _factory;

    public RequestExceptionHandlerProcessBehavior(ExceptionHandlerFactory factory) =>
        _factory = factory;

    public async ValueTask Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
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

                    var handler = _factory.CreateRequestExceptionRequestHandler(messageType, exceptionType);
                    await handler.HandleAsync(request, exception, state, cancellationToken);
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