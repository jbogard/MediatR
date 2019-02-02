using MediatR.Examples.Wrapper.Core;

namespace MediatR.Examples.Wrapper.Queries
{
    //Custom IQuery<TResponse> interface to allow you to remove direct MediatR refs
    public class PingQuery : IQuery<PongResponse>
    {
        public string Message { get; set; }
    }
}