namespace MediatR.Pipeline
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> instances after handling the request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestPostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> _postProcessors;
        private readonly ITaskExecutionStrategy _taskExecutionStrategy;

        public RequestPostProcessorBehavior(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> postProcessors, ITaskExecutionStrategy taskExecutionStrategy)
        {
            _postProcessors = postProcessors;
            _taskExecutionStrategy = taskExecutionStrategy;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var response = await next().ConfigureAwait(false);

            await _taskExecutionStrategy.Execute(_postProcessors.Select(p => p.Process(request, response))).ConfigureAwait(false);

            return response;
        }
    }
}
