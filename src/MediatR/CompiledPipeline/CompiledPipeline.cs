using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.CompiledPipeline;

// RequestHandlerDelegate seems not reusable here...?
public delegate Task<TOutput> PipelineDelegate<in TInput, TOutput>(
    TInput input, 
    CancellationToken cancellationToken = default)
    where TInput : class
    where TOutput : class;

public class CompiledPipeline<TInput, TOutput>
    where TInput : class
    where TOutput : class
{

    
    private readonly List<CompiledPipelineItem> _items = new();
    
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    private PipelineDelegate<TInput, TOutput>? _compiled;

    public PipelineDelegate<TInput, TOutput> Compiled<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        if (_compiled == null)
        {
            _compiled = Compile<TRequest,TResponse>();
        }

        return _compiled;
    }

    public PipelineDelegate<TInput, TOutput> Compile<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {

        var delegatesArr =
            _items
                .Cast<CompiledPipelineItem<TRequest, TResponse>>()
                .Select(item => item.AsDelegate())
                .ToArray();
        
        var pipelineItemsDelegate = (PipelineItemDelegate<TRequest, TResponse>) Delegate.Combine(delegatesArr)!;

        return (async (input, token) =>
        {
            TResponse? response = Activator.CreateInstance<TResponse>();
            var result = await pipelineItemsDelegate((TRequest)Convert.ChangeType(input, typeof(TRequest)), response, token);
            return (TOutput) Convert.ChangeType(result, typeof(TOutput));
        });
    }
    
    public CompiledPipeline(
        IMediator mediator,
        IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    public IQueryable<CompiledPipelineItem> QueryItems() => _items.AsQueryable();
    
    public CompiledPipelineItem RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse>? handler = default)
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        var item = new CompiledPipelineItem<TRequest, TResponse>();

        handler ??= _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        item.HandlerInstance = handler;
        item.HandlerType = handler?.GetType();
        item.HandlerMethodName = "Handle";

        item.ItemType = CompiledPipelineItem.ItemTypes.Handler;
        
        _items.Add(item);
        
        return item;
    }
    
    public CompiledPipelineItem RegisterPreProcessor<TRequest, TResponse>(IRequestPreProcessor<TRequest>? behavior = default)
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        var item = new CompiledPipelineItem<TRequest, TResponse>();
       
        behavior ??= _serviceProvider.GetRequiredService<IRequestPreProcessor<TRequest>>();
        item.HandlerInstance = behavior;
        item.HandlerType = behavior.GetType();
        item.HandlerMethodName = "Process";
        
        item.ItemType = CompiledPipelineItem.ItemTypes.PreProcessor;
        
        _items.Add(item);
        
        return item;
    }
    
    public CompiledPipelineItem RegisterPostProcessor<TRequest, TResponse>(IRequestPostProcessor<TRequest, TResponse>? behavior = default)
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        var item = new CompiledPipelineItem<TRequest, TResponse>();
       
        behavior ??= _serviceProvider.GetRequiredService<IRequestPostProcessor<TRequest, TResponse>>();
        item.HandlerInstance = behavior;
        item.HandlerType = behavior.GetType();
        item.HandlerMethodName = "Process";
        
        item.ItemType = CompiledPipelineItem.ItemTypes.PostProcessor;
        
        _items.Add(item);
        
        return item;
    }

    public bool DeregisterItem(CompiledPipelineItem item)
    {
        return _items.Remove(item);
    }

    public CompiledPipeline<TInput, TOutput> Prepare<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        for ( var i=0; i<_items.Count; i++)
        {
            var item = (CompiledPipelineItem<TRequest, TResponse>) _items[i];

            if (string.IsNullOrWhiteSpace(item.HandlerMethodName))
            {
                throw new ArgumentException("No method Handler name provided!");
            }

            var methods = item.HandlerType?.GetMethods();
            var methodInfo = methods?
                .FirstOrDefault(x => x.Name.Contains(item.HandlerMethodName));
            if (methodInfo == null)
            {
                throw new MissingMethodException($"No Method named '{item.HandlerMethodName}' found on Output type!");
            }

            if (methodInfo.ReturnParameter.ParameterType.Name.Contains(typeof(TOutput).Name))
            {
                throw new Exception("Type of return parameter does not match Output type!");
            }

            
            switch (item.ItemType)
            {
                case CompiledPipelineItem.ItemTypes.Handler:
                    item.PreparedHandler = BuildHandlerCallExpression(item, methodInfo);
                    break;
                case CompiledPipelineItem.ItemTypes.PreProcessor:
                    item.PreparedHandler = BuildPreProcessorCallExpression(item, methodInfo);
                    break;
                case CompiledPipelineItem.ItemTypes.PostProcessor:
                    item.PreparedHandler = BuildPostProcessorCallExpression(item, methodInfo);
                    break;
                case CompiledPipelineItem.ItemTypes.Unknown:
                default:
                    throw new NotImplementedException("Pipeline ItemType Unknown!");
            }
        }

        return this;
    }

    private Expression<Func<TRequest, CancellationToken, Task<TResponse>>> BuildHandlerCallExpression<TRequest, TResponse>(
        CompiledPipelineItem<TRequest, TResponse> item, 
        MethodInfo method)
            where TRequest : IRequest<TResponse>
            where TResponse : class
    {
        var parameters = method.GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType, p.Name))
            .ToArray();
        var instance = Expression.Constant(item.HandlerInstance);
        var call = Expression.Call(instance, method, parameters);
        return Expression.Lambda<Func<TRequest, CancellationToken, Task<TResponse>>>(call, parameters);
    }
    
    private Expression<Func<TRequest, CancellationToken, Task>> BuildPreProcessorCallExpression<TRequest, TResponse>(
        CompiledPipelineItem<TRequest, TResponse> item, 
        MethodInfo method)
            where TRequest : IRequest<TResponse>
            where TResponse : class
    {
        var parameters = method.GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType, p.Name))
            .ToArray();
        var instance = Expression.Constant(item.HandlerInstance);
        var call = Expression.Call(instance, method, parameters);
        return Expression.Lambda<Func<TRequest, CancellationToken, Task>>(call, parameters);
    }
    
    private Expression<Func<TRequest, TResponse, CancellationToken, Task>> BuildPostProcessorCallExpression<TRequest, TResponse>(
        CompiledPipelineItem<TRequest, TResponse> item, 
        MethodInfo method)
            where TRequest : IRequest<TResponse>
            where TResponse : class
    {
        var parameters = method.GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType, p.Name))
            .ToArray();
        var instance = Expression.Constant(item.HandlerInstance);
        var call = Expression.Call(instance, method, parameters);
        return Expression.Lambda<Func<TRequest, TResponse, CancellationToken, Task>>(call, parameters);
    }
}
