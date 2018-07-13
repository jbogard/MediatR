namespace MediatR.Pipeline
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestPreProcessor{TRequest}"/> instances before handling a request
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class RequestPreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IRequestPreProcessor<TRequest>> _preProcessors;
        private readonly ITaskExecutionStrategy _taskExecutionStrategy;

        public RequestPreProcessorBehavior(IEnumerable<IRequestPreProcessor<TRequest>> preProcessors, ITaskExecutionStrategy taskExecutionStrategy)
        {
            _preProcessors = preProcessors;
            _taskExecutionStrategy = taskExecutionStrategy;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            await _taskExecutionStrategy.Execute((_preProcessors.Select(p => p.Process(request, cancellationToken)))).ConfigureAwait(false);

            return await next().ConfigureAwait(false);
        }
    }
}
