using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Abstraction.Behaviors;

namespace MediatR.Examples;

public class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly TextWriter _writer;

    public GenericPipelineBehavior(TextWriter writer)
    {
        _writer = writer;
    }

    public async ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("-- Handling Request Response");
        var response = await next(request, cancellationToken);
        await  _writer.WriteLineAsync("-- Finished Request Resposne");
        return response;
    }
}

public class GenericPipelineBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly TextWriter _writer;

    public GenericPipelineBehavior(TextWriter writer)
    {
        _writer = writer;
    }

    public async ValueTask Handle(TRequest request, RequestHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync("-- Handling Request");
        await next(request, cancellationToken);
        await  _writer.WriteLineAsync("-- Finished Request");
    }
}