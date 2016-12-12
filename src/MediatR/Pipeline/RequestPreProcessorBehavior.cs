namespace MediatR.Pipeline
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class RequestPreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IRequestPreProcessor<TRequest>> _preProcessors;

        public RequestPreProcessorBehavior(IEnumerable<IRequestPreProcessor<TRequest>> preProcessors)
        {
            _preProcessors = preProcessors;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
        {
            await Task.WhenAll(_preProcessors.Select(p => p.Process(request)));

            return await next();
        }
    }
}