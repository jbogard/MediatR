namespace MediatR.Composite
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Dynamic;
    using System.Collections.Generic;

    /// <summary>
    /// defines an asynchronous handler for AsyncCompositeRequest <see cref="AsyncCompositeRequest"/>
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
}
