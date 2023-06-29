using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExceptionHandling;

internal sealed class RequestResponseExceptionActionProcessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Type ResponseType = typeof(TResponse);
    private static readonly Type[] RequestResponseTypeHierarchy = MessageTypeResolver.MessageTypeHierarchyFactory(typeof(TRequest));
    
    private readonly ExceptionHandlerFactory _factory;

    public RequestResponseExceptionActionProcessBehavior(ExceptionHandlerFactory exceptionHandlerFactory) =>
        _factory = exceptionHandlerFactory;

    public async ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            foreach (var exceptionType in ExceptionTypeResolver.GetExceptionTypeHierarchy(exception.GetType()))
            {
                foreach (var messageType in RequestResponseTypeHierarchy)
                {
                    var handler = _factory.CreateRequestResponseExceptionActionHandler(messageType, ResponseType, exceptionType);
                    await handler.Handle(request, exception, cancellationToken);
                }
            }

            throw;
        }
    }
}