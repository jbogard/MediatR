namespace MediatR.Composite
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Dynamic;
    using System.Collections.Generic;

    /// <summary>
    /// Defines an asynchronous handler for DynamicCompositeRequests <see cref="DynamicCompositeRequest"/>
    /// </summary>
    /// <remarks>Output will always be an ExpandoObject <see cref="ExpandoObject"/></remarks>
    /// <typeparam name="TCompositeRequest">The type of request being handled, must inherit from DynamicCompositeRequest</typeparam>
    public abstract class AsyncDynamicCompositeRequestHandler<TCompositeRequest> : IRequestHandler<TCompositeRequest, ExpandoObject>
    where TCompositeRequest : DynamicCompositeRequest
    {
        private IMediator _mediator;

        public AsyncDynamicCompositeRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ExpandoObject> Handle(TCompositeRequest compositeRequest, CancellationToken cancellationToken)
        {
            IDictionary<string, object> response = new ExpandoObject();
            var shouldMerge = compositeRequest.MergeRepsonses();

            foreach (var request in compositeRequest.Requests())
            {
                if (shouldMerge)
                    response = Merge(response, await _mediator.Send(request));
                else
                    response[request.GetType().Name] = await _mediator.Send(request);
            }

            return response as ExpandoObject;
        }

        internal virtual IDictionary<string, object> Merge(IDictionary<String, Object> left, object right)
        {
            foreach (var property in right.GetType().GetProperties())
            {
                left[property.Name] = property.GetValue(right, null);
            }

            return left;
        }
    }


    /// <summary>
    /// Defines a synchronous handler for DynamicCompositeRequests <see cref="DynamicCompositeRequest"/>
    /// </summary>
    /// <remarks>Output will always be an ExpandoObject <see cref="ExpandoObject"/></remarks>
    /// <typeparam name="TCompositeRequest">The type of request being handled, must inherit from DynamicCompositeRequest</typeparam>
    public abstract class DynamicCompositeRequestHandler<TCompositeRequest> : IRequestHandler<TCompositeRequest, ExpandoObject>
    where TCompositeRequest : DynamicCompositeRequest
    {
        private IMediator _mediator;

        public DynamicCompositeRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<ExpandoObject> Handle(TCompositeRequest compositeRequest, CancellationToken cancellationToken)
        {
            IDictionary<string, object> response = new ExpandoObject();
            var shouldMerge = compositeRequest.MergeRepsonses();

            foreach (var request in compositeRequest.Requests())
            {
                if (shouldMerge)
                    response = Merge(response, _mediator.Send(request));
                else
                    response[request.GetType().Name] = _mediator.Send(request);
            }

            return Task.FromResult(response as ExpandoObject);
        }

        internal virtual IDictionary<string, object> Merge(IDictionary<String, Object> left, object right)
        {
            foreach (var property in right.GetType().GetProperties())
            {
                left[property.Name] = property.GetValue(right, null);
            }

            return left;
        }
    }
}
