namespace MediatR.Composite
{
    using System.Collections.Generic;
    using System.Dynamic;

    /// <summary>
    /// Marker interface to represent a request with a dynamic aggregate responses from multiple requests
    /// </summary>
    public abstract class DynamicCompositeRequest : IRequest<ExpandoObject>
    {
        /// <summary>
        /// The requests to be processed by the composite handler
        /// </summary>
        /// <returns>A list of IRequests</returns>
        /// <example>
        /// public override IEnumerable<IBaseRequest> Requests()
        /// {
        ///     yield return new Ping(); // public class Ping : IRequest
        ///     yield return new Foo();  // public class Foo : IRequest
        /// }
        /// </example>
        public abstract IEnumerable<IBaseRequest> Requests();


        /// <summary>
        /// Whether the dynamic output of the responses should be merged
        /// </summary>
        /// <returns>boolean - default false</returns>
        /// <example>
        /// // merge = true
        /// dynamic response = await mediator.Send(new CompositeRequest());
        /// Assert.Equal(response.Message, "Ping Pong");
        /// Assert.Equal(response.Total, 9001);
        ///
        /// // merge = false
        /// dynamic response = await mediator.Send(new CompositeRequest());
        /// Assert.Equal(response.Ping.Message, "Ping Pong");
        /// Assert.Equal(response.Foo.Total, 9001);
        /// </example>
        public virtual bool MergeRepsonses()
        {
            return false;
        }
    }
}
