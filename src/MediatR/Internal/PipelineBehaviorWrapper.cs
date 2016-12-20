namespace MediatR.Internal
{
    using System.Linq;
    using System.Threading.Tasks;

    internal abstract class PipelineBehaviorWrapper<TResponse>
    {
        public abstract Task<TResponse> CreatePipeline(object request, RequestHandlerDelegate<TResponse> invokeHandler, MultiInstanceFactory factory);
    }

    internal class PipelineBehaviorWrapper<TRequest, TResponse> 
        : PipelineBehaviorWrapper<TResponse>
    {
        public override Task<TResponse> CreatePipeline(object request, 
            RequestHandlerDelegate<TResponse> invokeHandler, 
            MultiInstanceFactory factory)
        {
            var behaviors = factory(typeof(IPipelineBehavior<TRequest, TResponse>))
                .Cast<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse();

            var aggregate = behaviors.Aggregate(invokeHandler, (next, pipeline) => () => pipeline.Handle((TRequest)request, next));

            return aggregate();
        }
    }
}
