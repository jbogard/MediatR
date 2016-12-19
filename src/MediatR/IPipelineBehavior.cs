namespace MediatR
{
    using System;
    using System.Threading.Tasks;

    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    public interface IPipelineBehavior<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next);
    }
}