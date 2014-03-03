namespace MediatR
{
    public interface ICommand : ICommand<Unit> { }
    public interface ICommand<out TResult> { }
    public interface IQuery<out TResponse> { }
}