using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Processors;

namespace MediatR.Pipeline.RequestResponse;

internal sealed class TransientRequestResponseProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public TransientRequestResponseProcessorBehavior(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        foreach (var requestPreProcessor in _serviceProvider.GetServices<IRequestPreProcessor<TRequest, TResponse>>())
        {
            await requestPreProcessor.Process(request, cancellationToken).ConfigureAwait(false);
        }

        var response = await next(request, cancellationToken).ConfigureAwait(false);

        foreach (var requestPostProcessor in _serviceProvider.GetServices<IRequestPostProcessor<TRequest, TResponse>>())
        {
            await requestPostProcessor.Process(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}