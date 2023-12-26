using System.Linq;
using MediatR.Internal;

namespace MediatR.Pipeline;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Behavior for executing all <see cref="IRequestPreProcessor{TRequest}"/> instances before handling a request
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class RequestPreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestPreProcessor<TRequest>> _preProcessors;

    public RequestPreProcessorBehavior(IEnumerable<IRequestPreProcessor<TRequest>> preProcessors) 
        => _preProcessors = preProcessors;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        foreach (var processor in _preProcessors.OrderBy(GetOrder))
        {
            await processor.Process(request, cancellationToken).ConfigureAwait(false);
        }

        return await next().ConfigureAwait(false);
    }

    private static int GetOrder(IRequestPreProcessor<TRequest> arg)
    {
        if (arg is IOrderableHandler oh)
            return oh.Order;

        return arg.GetOrderIfExists().GetValueOrDefault(0);
    }
}