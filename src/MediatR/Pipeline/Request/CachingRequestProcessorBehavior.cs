using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Processors;

namespace MediatR.Pipeline.Request;

internal sealed class CachingRequestProcessorBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly IRequestPreProcessor<TRequest>[] _preProcessors;
    private readonly IRequestPostProcessor<TRequest>[] _postProcessors;

    public CachingRequestProcessorBehavior(IServiceProvider serviceProvider)
    {
        _preProcessors = serviceProvider.GetServices<IRequestPreProcessor<TRequest>>();
        _postProcessors = serviceProvider.GetServices<IRequestPostProcessor<TRequest>>();
    }

    public async Task Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        foreach (var requestPreProcessor in _preProcessors)
        {
            await requestPreProcessor.Process(request, cancellationToken);
        }

        await next(request, cancellationToken);

        foreach (var requestPostProcessor in _postProcessors)
        {
            await requestPostProcessor.Process(request, cancellationToken);
        }
    }
}