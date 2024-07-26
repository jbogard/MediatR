namespace MediatR.Pipeline;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Behavior for executing all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> instances after handling the request
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class RequestPostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> _postProcessors;

    public RequestPostProcessorBehavior(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> postProcessors)
        => _postProcessors = postProcessors ?? throw new ArgumentNullException(nameof(postProcessors));

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (next is null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        var response = await next().ConfigureAwait(false);

        foreach (var processor in _postProcessors)
        {
            await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}