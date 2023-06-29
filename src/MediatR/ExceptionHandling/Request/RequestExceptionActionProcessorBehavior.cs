using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExceptionHandling;

internal sealed class RequestExceptionActionProcessorBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private static readonly Type[] RequestExceptionType = MessageTypeResolver.MessageTypeHierarchyFactory(typeof(TRequest));
    
    private readonly ExceptionHandlerFactory _factory;

    public RequestExceptionActionProcessorBehavior(ExceptionHandlerFactory factory) =>
        _factory = factory;

    public async ValueTask Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        try
        {
            await next(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            foreach (var exceptionType in ExceptionTypeResolver.GetExceptionTypeHierarchy(exception.GetType()))
            {
                foreach (var messageType in RequestExceptionType)
                {
                    var handler = _factory.CreateRequestExceptionActionHandler(messageType, exceptionType);
                    await handler.HandleAsync(request, exception, cancellationToken);
                }
            }

            throw;
        }
    }
}