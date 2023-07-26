using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Processors;

namespace MediatR.Pipeline.RequestResponse;

internal sealed class CachingRequestResponseProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRequestPreProcessor<TRequest, TResponse>[] _preProcessors;
    private readonly IRequestPostProcessor<TRequest, TResponse>[] _postProcessors;

    public CachingRequestResponseProcessorBehavior(IServiceProvider serviceProvider)
    {
        _preProcessors = serviceProvider.GetServices<IRequestPreProcessor<TRequest, TResponse>>();
        _postProcessors = serviceProvider.GetServices<IRequestPostProcessor<TRequest, TResponse>>();
    }

    public async ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        foreach (var requestPreProcessor in _preProcessors)
        {
            await requestPreProcessor.Process(request, cancellationToken).ConfigureAwait(false);
        }

        var response = await next(request, cancellationToken).ConfigureAwait(false);

        foreach (var requestPostProcessor in _postProcessors)
        {
            await requestPostProcessor.Process(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}