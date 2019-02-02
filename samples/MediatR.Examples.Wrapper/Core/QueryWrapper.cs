namespace MediatR.Examples.Wrapper.Core
{
    public class QueryWrapper<TQuery, TResponse> : IRequest<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public TQuery Query { get; set; }
    }
}