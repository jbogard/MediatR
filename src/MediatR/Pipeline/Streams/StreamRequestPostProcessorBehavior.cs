namespace MediatR.Pipeline.Streams
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IStreamRequestPostProcessor{TRequest,TResponse}"/> instances after handling the request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class StreamRequestPostProcessorBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IStreamRequestPostProcessor<TRequest, TResponse>> _postProcessors;

        public StreamRequestPostProcessorBehavior(IEnumerable<IStreamRequestPostProcessor<TRequest, TResponse>> postProcessors)
        {
            _postProcessors = postProcessors;
        }

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, [EnumeratorCancellation]CancellationToken cancellationToken, StreamHandlerDelegate<TResponse> next)
        {
            await foreach (var response in next().WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                // Run postprocessing for each item in stream...
                foreach (var processor in _postProcessors)
                {
                    await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
                }

                // Pass stream results forwards...
                yield return response;
            }
        }
    }
}
