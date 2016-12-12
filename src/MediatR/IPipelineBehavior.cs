namespace MediatR
{
    using System;
    using System.Threading.Tasks;

    public delegate Task<object> RequestHandlerDelegate();

    public interface IPipelineBehavior
    {
        Task<object> Handle(object request, RequestHandlerDelegate next);
    }
}