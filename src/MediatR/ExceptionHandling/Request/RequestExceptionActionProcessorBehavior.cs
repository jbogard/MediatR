using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExceptionHandling.Request;

internal sealed class RequestExceptionActionProcessorBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private static readonly Type[] RequestExceptionType = MessageTypeResolver.GetMessageTypeHierarchy(typeof(TRequest));

    private readonly IServiceProvider _serviceProvider;

    public RequestExceptionActionProcessorBehavior(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
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
                    var handler = ExceptionHandlerFactory.CreateRequestExceptionActionHandler(messageType, exceptionType);
                    await handler.HandleAsync(request, exception, _serviceProvider, cancellationToken);
                }
            }

            throw;
        }
    }
}