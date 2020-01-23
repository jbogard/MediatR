namespace MediatR.Composite
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    /// <summary>
    /// represents an asynchronous request with a dynamic aggregate responses from multiple requests
    /// </summary>
    public class AsyncCompositeRequest : IRequest<ExpandoObject>
    {
        /// <example>
        /// IEnumerable<IBaseRequest> GetRequestList()
        //  {
        //    yield return new Ping();
        //    yield return new Foo();
        //  }
        //
        //  dynamic responseFromFunc = await mediator.Send(new AsyncCompositeRequest(GetRequestList));
        /// </exmple>
        public AsyncCompositeRequest(Func<IEnumerable<IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        /// <example>
        //  dynamic response = await mediator.Send(
        //    new AsyncCompositeRequest(new List<IBaseRequest> { new Ping(), new Foo() } )
        //  );
        /// </exmple>
        public AsyncCompositeRequest(IEnumerable<IBaseRequest> requests) : this(requests.ToList().ToDictionary(x => x.GetType().Name))
        {
        }

        /// <example>
        /// IEnumerable<IBaseRequest> GetRequestList()
        //  {
        //      return new Dictionary<string, IBaseRequest>()
        //      {
        //          { "Ping", new Ping() },
        //          { "Foo",  new Foo()  }
        //      };
        //  }
        //  dynamic responseFromFunc = await mediator.Send(new AsyncCompositeRequest(GetRequestList));
        /// </exmple>
        public AsyncCompositeRequest(Func<Dictionary<string, IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        /// <example>
        /// dynamic response = await mediator.Send(new AsyncCompositeRequest(
        //      new Dictionary<string, IBaseRequest>()
        //      {
        //          { "Ping", new Ping() },
        //          { "Foo",  new Foo()  }
        //      }
        //  ));
        /// </exmple>
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
    /// represents a synchronous request with a dynamic aggregate responses from multiple requests
    /// </summary>
    public class CompositeRequest : IRequest<ExpandoObject>
    {
        /// <example>
        /// IEnumerable<IBaseRequest> GetRequestList()
        //  {
        //    yield return new Ping();
        //    yield return new Foo();
        //  }
        //
        //  dynamic responseFromFunc = await mediator.Send(new CompositeRequest(GetRequestList));
        /// </exmple>
        public CompositeRequest(Func<IEnumerable<IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        /// <example>
        //  dynamic response = await mediator.Send(
        //    new CompositeRequest(new List<IBaseRequest> { new Ping(), new Foo() } )
        //  );
        /// </exmple>
        public CompositeRequest(IEnumerable<IBaseRequest> requests) : this(requests.ToList().ToDictionary(x => x.GetType().Name))
        {
        }

        /// <example>
        /// IEnumerable<IBaseRequest> GetRequestList()
        //  {
        //      return new Dictionary<string, IBaseRequest>()
        //      {
        //          { "Ping", new Ping() },
        //          { "Foo",  new Foo()  }
        //      };
        //  }
        //  dynamic responseFromFunc = await mediator.Send(new CompositeRequest(GetRequestList));
        /// </exmple>
        public CompositeRequest(Func<Dictionary<string, IBaseRequest>> requestsFunc) : this(requestsFunc.Invoke())
        {
        }

        /// <example>
        /// dynamic response = await mediator.Send(new CompositeRequest(
        //      new Dictionary<string, IBaseRequest>()
        //      {
        //          { "Ping", new Ping() },
        //          { "Foo",  new Foo()  }
        //      }
        //  ));
        /// </exmple>
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
