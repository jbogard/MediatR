namespace MediatR
{
    public interface IRequest : IRequest<Unit> { }
    public interface IAsyncRequest : IAsyncRequest<Unit> { }
    public interface IRequest<out TResponse> { }
    public interface IAsyncRequest<out TResponse> { }
    public interface INotification { }
    public interface IAsyncNotification { }
}