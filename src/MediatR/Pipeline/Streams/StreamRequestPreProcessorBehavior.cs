namespace MediatR.Pipeline.Streams
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestPreProcessor{TRequest}"/> instances before handling a request
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class StreamRequestPreProcessorBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IStreamRequestPreProcessor<TRequest>> _preProcessors;

        public StreamRequestPreProcessorBehavior(IEnumerable<IStreamRequestPreProcessor<TRequest>> preProcessors)
        {
            _preProcessors = preProcessors;
        }

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, [EnumeratorCancellation] CancellationToken cancellationToken, StreamHandlerDelegate<TResponse> next)
        {
            // Only preprocess once at the beginning of the stream...
            foreach (var processor in _preProcessors)
            {
                await processor.Process(request, cancellationToken);
            }

            // Pass stream items forwards...
            await foreach (var item in next().WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }

    }
}
