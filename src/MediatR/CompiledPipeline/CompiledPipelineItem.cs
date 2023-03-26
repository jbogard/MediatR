using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR.CompiledPipeline.Extensions;

namespace MediatR.CompiledPipeline;


public class CompiledPipelineItem
{
    public enum ItemTypes
    {
        Unknown = 0,
        Handler = 1,
        PreProcessor = 2,
        PostProcessor = 3
    }
    
    protected Expression? BasePreparedHandler;
    
    
    public Type? RequestType { get; set; }
    public IBaseRequest? RequestInstance { get; set; }

    public Type? HandlerType { get; set; }
    public object? HandlerInstance { get; set; }

    public string? HandlerMethodName { get; set; } = "Handle";

    public ItemTypes ItemType { get; set; }
    
    public Type? ResponseType { get; set; }
    public object? ResponseInstance { get; set; }
}

public class CompiledPipelineItem<TRequest, TResponse> : CompiledPipelineItem
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public TRequest Request
    {
        get
        {
            if (RequestInstance == null)
                throw new ArgumentNullException(nameof(RequestInstance));
            
            return (TRequest) RequestInstance;
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            RequestType = value.GetType();
            RequestInstance = value;
        }
    }

    public IRequestHandler<TRequest, TResponse> Handler
    {
        get
        {
            if (HandlerInstance == null)
                throw new ArgumentNullException(nameof(HandlerInstance));
            
            return (IRequestHandler<TRequest, TResponse>) HandlerInstance;
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            HandlerType = value.GetType();
            HandlerInstance = value;
        }
    }

    public TResponse Response
    {
        get
        {
            if (ResponseInstance == null)
                throw new ArgumentNullException(nameof(ResponseInstance));
            
            return (TResponse) ResponseInstance;
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            ResponseType = value.GetType();
            ResponseInstance = value;
        }
    }


    public Expression? PreparedHandler
    {
        get => BasePreparedHandler;

        set =>  BasePreparedHandler = value;
    }

    public Expression<Func<TRequest, CancellationToken, Task<TResponse>>>? AsHandlerExpression()
    {
        return (Expression<Func<TRequest, CancellationToken, Task<TResponse>>>?) PreparedHandler;
    }
    public PipelineDelegate<TRequest, TResponse>? AsHandlerDelegate()
    {
        var myHandler = AsHandlerExpression()!.Compile();
        return (request, response, cancellationToken) =>
        {
            var result = myHandler(request, cancellationToken).Result;
            (result!).CopyPropertiesTo((response!));
            return Task.FromResult(response!);
        };
    }

    public Expression<Func<TRequest, CancellationToken, Task>>? AsPreProcessorExpression()
    {
        return (Expression<Func<TRequest, CancellationToken, Task>>?) PreparedHandler;
    }
    public PipelineDelegate<TRequest, TResponse>? AsPreProcessorDelegate()
    {
        var myFunc = AsPreProcessorExpression()!.Compile();
        return ((request, response, cancellationToken) =>
        {
            myFunc(request, cancellationToken);
            return Task.FromResult(response!);
        });
    }
    
    public Expression<Func<TRequest, TResponse, CancellationToken, Task>>? AsPostProcessorExpression()
    {
        return (Expression<Func<TRequest, TResponse, CancellationToken, Task>>?) PreparedHandler;
    }
    public PipelineDelegate<TRequest, TResponse>? AsPostProcessorDelegate()
    {
        var myFunc = AsPostProcessorExpression()!.Compile();
        return (request, response, cancellationToken) =>
        {
            myFunc(request, response!, cancellationToken);
            return Task.FromResult(response!);
        };
    }

    public PipelineDelegate<TRequest, TResponse>? AsDelegate()
    {
        switch (this.ItemType)
        {
            case ItemTypes.Handler:
                return AsHandlerDelegate();
            case ItemTypes.PreProcessor:
                return AsPreProcessorDelegate();
            case ItemTypes.PostProcessor:
                return AsPostProcessorDelegate();
            case ItemTypes.Unknown:
            default:
                throw new NotImplementedException("ItemType requested is not implemented!");
        }
    }
}