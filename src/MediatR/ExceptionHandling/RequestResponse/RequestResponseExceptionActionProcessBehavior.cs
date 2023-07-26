using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.ExceptionHandling.RequestResponse;

internal sealed class RequestResponseExceptionActionProcessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly Type ResponseType = typeof(TResponse);
    private static readonly Type[] RequestResponseTypeHierarchy = MessageTypeResolver.GetMessageTypeHierarchy(typeof(TRequest));

    private readonly IServiceProvider _serviceProvider;

    public RequestResponseExceptionActionProcessBehavior(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
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
                    var handler = ExceptionHandlerFactory.CreateRequestResponseExceptionActionHandler(messageType, ResponseType, exceptionType);
                    await handler.Handle<TRequest, TResponse>(request, exception, _serviceProvider, cancellationToken);
                }
            }

            throw;
        }
    }
}