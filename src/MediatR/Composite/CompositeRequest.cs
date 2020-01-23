namespace MediatR.Composite
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    /// <summary>
    /// represents a request with a dynamic aggregate responses from multiple requests
    /// </summary>
    public class AsyncCompositeRequest : IRequest<ExpandoObject>
    {
        public AsyncCompositeRequest(Func<IEnumerable<IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        public AsyncCompositeRequest(IEnumerable<IBaseRequest> requests) : this(requests.ToList().ToDictionary(x => x.GetType().Name))
        {
        }

        public AsyncCompositeRequest(Func<Dictionary<string, IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        public AsyncCompositeRequest(Dictionary<string, IBaseRequest> requests)
        {
            Requests = requests;
        }


        /// <summary>
        /// The requests to be processed by the composite handler
        /// </summary>
        public Dictionary<string, IBaseRequest> Requests { get; }

    }



    /// <summary>
    /// represents a request with a dynamic aggregate responses from multiple requests
    /// </summary>
    public class CompositeRequest : IRequest<ExpandoObject>
    {
        public CompositeRequest(Func<IEnumerable<IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        public CompositeRequest(IEnumerable<IBaseRequest> requests) : this(requests.ToList().ToDictionary(x => x.GetType().Name))
        {
        }

        public CompositeRequest(Func<Dictionary<string, IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        public CompositeRequest(Dictionary<string, IBaseRequest> requests)
        {
            Requests = requests;
        }


        /// <summary>
        /// The requests to be processed by the composite handler
        /// </summary>
        public Dictionary<string, IBaseRequest> Requests { get; }

    }
}
