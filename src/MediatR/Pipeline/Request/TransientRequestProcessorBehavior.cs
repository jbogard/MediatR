using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Processors;

namespace MediatR.Pipeline.Request;

internal sealed class TransientRequestProcessorBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly IServiceProvider _serviceProvider;

    public TransientRequestProcessorBehavior(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        foreach (var requestPreProcessor in _serviceProvider.GetServices<IRequestPreProcessor<TRequest>>())
        {
            await requestPreProcessor.Process(request, cancellationToken);
        }

        await next(request, cancellationToken);

        foreach (var requestPostProcessor in _serviceProvider.GetServices<IRequestPostProcessor<TRequest>>())
        {
            await requestPostProcessor.Process(request, cancellationToken);
        }
    }
}