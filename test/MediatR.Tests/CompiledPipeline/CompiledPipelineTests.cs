using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Lamar;
using LamarCodeGeneration.Util;
using MediatR.Pipeline;
using Shouldly;
using Xunit;

namespace MediatR.Tests.CompiledPipeline;

using MediatR.CompiledPipeline;

public class Ping : IRequest<Pong>
{
    public string? Message { get; set; }
}

public class Pong
{
    public string? Message { get; set; }
}


public class MyHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
       cancellationToken.ThrowIfCancellationRequested();

       return Task.FromResult(new Pong { Message = $"{request.Message} Smash!" });
    }
}

public class MyPreProcessor : IRequestPreProcessor<Ping>
{
    public Task Process(Ping request, CancellationToken cancellationToken)
    {
        request.Message = $"Serving {request.Message} ";

        return Task.CompletedTask;
    }
}

public class MyPostProcessor : IRequestPostProcessor<Ping, Pong>
{
    public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
    {
        response.Message += " backhand ";

        return Task.CompletedTask;
    }
}

public class CompiledPipelineTests
{

    [Fact]
    public void Should_compile_and_run_pipeline_one_handler()
    {
        // Arrange
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CompiledPipelineTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var compiledPipeline = new CompiledPipeline(mediator, container);
        
        // Act
        compiledPipeline.RegisterHandler<Ping, Pong>();
        var result = compiledPipeline.Prepare<Ping, Pong>();

        // Assert
        result.ShouldNotBeNull();
        
        var item = (CompiledPipelineItem<Ping, Pong>?)compiledPipeline.QueryItems().FirstOrDefault();
        item.ShouldNotBeNull();
        item.PreparedHandler.ShouldNotBeNull();

        compiledPipeline.QueryItems().Count().ShouldBe(1);
        
        CompiledPipelineItem<Ping, Pong> handlerItem = (CompiledPipelineItem<Ping,Pong>)compiledPipeline.QueryItems().First();
        handlerItem.ItemType.ShouldBe(CompiledPipelineItem.ItemTypes.Handler);

        var handler = handlerItem.AsHandlerExpression();
        handler.ShouldNotBeNull();

        var f = handler.Compile();
        var res = f(new Ping { Message = " me " }, CancellationToken.None);

        res.ShouldNotBeNull();
        res.Result.ShouldNotBeNull();
        res.Result.Message.ShouldBe(" me  Smash!");
    }

    [Fact]
    public async void Should_run_pre_handler_post_without_compiled_pipeline()
    {
        // Arrange
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CompiledPipelineTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestPreProcessor<>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
            });
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        // Act
        var result = await mediator.Send(new Ping { Message = " me " });
        
        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe("Serving  me   Smash! backhand "); 
    }
   
    [Fact]
    public async Task Should_compile_and_run_pipeline_pre_and_post_handlers()
    {
        // Arrange
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CompiledPipelineTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestPreProcessor<>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
            });
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var pipeline = new CompiledPipeline(mediator, container);

        // Act
        pipeline.RegisterPreProcessor<Ping, Pong>();
        pipeline.RegisterHandler<Ping, Pong>();
        pipeline.RegisterPostProcessor<Ping, Pong>();

        var result = pipeline.Prepare<Ping, Pong>();

        var preItem = (CompiledPipelineItem<Ping, Pong>?) pipeline.QueryItems().FirstOrDefault();
        preItem?.ItemType.ShouldBe(CompiledPipelineItem.ItemTypes.PreProcessor);

        var handlerItem = (CompiledPipelineItem<Ping, Pong>?) pipeline.QueryItems().Skip(1).FirstOrDefault();
        handlerItem?.ItemType.ShouldBe(CompiledPipelineItem.ItemTypes.Handler);

        var postItem = (CompiledPipelineItem<Ping, Pong>?) pipeline.QueryItems().LastOrDefault();
        postItem?.ItemType.ShouldBe(CompiledPipelineItem.ItemTypes.PostProcessor);


        // Assert
        result.ShouldNotBeNull();
        pipeline.QueryItems().Count().ShouldBe(3);

        var preProcessor = preItem?.AsPreProcessorExpression()?.Compile();
        preProcessor.ShouldNotBeNull();

        var handler = handlerItem?.AsHandlerExpression()?.Compile();
        handler.ShouldNotBeNull();

        var postProcessor = postItem?.AsPostProcessorExpression()?.Compile();
        postProcessor.ShouldNotBeNull();

        var ping = new Ping { Message = " me " };
        var cancellationTokenSource = new CancellationTokenSource();

        var preResult = preProcessor(ping, cancellationTokenSource.Token);
        var handlerResult = handler(ping, cancellationTokenSource.Token);
        var postResult = postProcessor(ping, handlerResult.Result, cancellationTokenSource.Token);

        var res = handlerResult.Result;
        res.ShouldNotBeNull();
        res.Message.ShouldNotBeNull();
        res.Message.ShouldBe("Serving  me   Smash! backhand ");


        var pipelineFunc = new PipelineDelegate<Ping, Pong>(delegate(Ping myPing, Pong? myPong, CancellationToken myCancellationToken)
        {
            preProcessor(myPing, myCancellationToken);
            myPong = handler(myPing, myCancellationToken).Result;
            postProcessor(myPing, myPong, myCancellationToken);

            return Task.FromResult(myPong);
        });

        var ping2 = new Ping { Message = " me " };
        var pong2 = new Pong();
        var resPipelineFunc = await pipelineFunc(ping2, pong2, cancellationTokenSource.Token);
        resPipelineFunc.ShouldNotBeNull();
        resPipelineFunc.Message.ShouldNotBeNull();
        resPipelineFunc.Message.ShouldBe("Serving  me   Smash! backhand ");


        var ping3 = new Ping { Message = " me " };
        var pipelineDelegates = (PipelineDelegate<Ping, Pong>)Delegate.Combine(
            new PipelineDelegate<Ping, Pong>(delegate (Ping myPing, Pong? myPong, CancellationToken myCancellationToken)
            {
                preProcessor(myPing, myCancellationToken);
                
                return Task.FromResult(myPong!);
            }),
            new PipelineDelegate<Ping, Pong>(delegate (Ping myPing, Pong? myPong, CancellationToken myCancellationToken)
            {
                myPong!.Message = (handler(myPing, myCancellationToken)).GetAwaiter().GetResult().Message;

                return Task.FromResult(myPong!);
            }),
            new PipelineDelegate<Ping, Pong>(delegate (Ping myPing, Pong? myPong, CancellationToken myCancellationToken)
            {
                postProcessor(myPing, myPong!, myCancellationToken);
                
                return Task.FromResult(myPong!);
            }))!;

        var pong3 = new Pong();
        var resPipelineDelegates = (Task<Pong?>)pipelineDelegates!.DynamicInvoke(ping3, pong3, cancellationTokenSource.Token)!;


        //var resPipelineDelegatesFunc = new Func<Ping, Pong?, CancellationToken, Task<Pong?>>(pipelineDelegates.ToFunc(pong3));
        var ping4 = new Ping { Message = " you " };
        var pong4 = new Pong();
        //var resPipelineDelegatesFunc = pipelineDelegates.ToFunc(pong4);
        var resPipelineDelegatesResult = await pipelineDelegates(ping4, pong4, cancellationTokenSource.Token);
        
        resPipelineDelegatesResult.ShouldNotBeNull();
        resPipelineDelegatesResult.Message.ShouldNotBeNull();
        resPipelineDelegatesResult.Message.ShouldBe("Serving  you   Smash! backhand ");
    }
    
    [Fact]
    public async Task Should_compile_and_run_pipeline_pre_handler_post()
    {
        // Arrange
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CompiledPipelineTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestPreProcessor<>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
            });
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var pipeline = new CompiledPipeline(mediator, container);

        // Act
        pipeline.RegisterPreProcessor<Ping, Pong>();
        pipeline.RegisterHandler<Ping, Pong>();
        pipeline.RegisterPostProcessor<Ping, Pong>();

        var compiledPipeline = pipeline.Prepare<Ping, Pong>().Compile<Ping, Pong>();

        // Assert
        compiledPipeline.ShouldNotBeNull();

        var ping = new Ping { Message = " me " };
        var pong = new Pong();
        var cancellationTokenSource = new CancellationTokenSource();

        var res = await compiledPipeline.Compiled<Ping, Pong>()!(ping, pong, cancellationTokenSource.Token);
        
        res.ShouldNotBeNull();
        res.Message.ShouldNotBeNull();
        res.Message.ShouldBe("Serving  me   Smash! backhand ");
    }
}
