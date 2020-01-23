namespace MediatR.Composite
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Dynamic;
    using System.Collections.Generic;

    /// <summary>
    /// Defines an asynchronous handler for DynamicCompositeRequests <see cref="CompositeRequest"/>
    /// </summary>
    /// <remarks>Output will always be an ExpandoObject <see cref="ExpandoObject"/></remarks>
    public class AsyncCompositeRequestHandler : IRequestHandler<AsyncCompositeRequest, ExpandoObject>
    {
        private IMediator _mediator;

        public AsyncCompositeRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ExpandoObject> Handle(AsyncCompositeRequest compositeRequest, CancellationToken cancellationToken)
        {
            IDictionary<string, object> response = new ExpandoObject();
            foreach (var request in compositeRequest.Requests)
            {
                response[request.Key] = await _mediator.Send(request.Value);
            }

            return response as ExpandoObject;
        }
    }



    /// <summary>
    /// Defines a synchronous handler for DynamicCompositeRequests <see cref="CompositeRequest"/>
    /// </summary>
    /// <remarks>Output will always be an ExpandoObject <see cref="ExpandoObject"/></remarks>
    /// <typeparam name="TCompositeRequest">The type of request being handled, must inherit from DynamicCompositeRequest</typeparam>
    public class CompositeRequestHandler : IRequestHandler<CompositeRequest, ExpandoObject>
    {
        private IMediator _mediator;

        public CompositeRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<ExpandoObject> Handle(CompositeRequest compositeRequest, CancellationToken cancellationToken)
        {
            IDictionary<string, object> response = new ExpandoObject();
            foreach (var request in compositeRequest.Requests)
            {
                response[request.Key] = _mediator.Send(request.Value);
            }

            return Task.FromResult(response as ExpandoObject);
        }
    }
}
