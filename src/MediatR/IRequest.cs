namespace MediatR
{
    public interface IRequest : IRequest<Unit> { }
    public interface IRequest<out TResponse> { }
}