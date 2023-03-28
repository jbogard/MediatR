using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lamar;
using MediatR.Pipeline;
using Shouldly;
using Xunit;

namespace MediatR.Tests.CompiledPipeline;

using MediatR.CompiledPipeline;

public class CompiledPipelineTests
{
    public CompiledPipelineTests()
    {
    }

    [Fact]
    public void Should_compile_and_run_pipeline_one_handler()
    {
        // Arrange
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(CompiledPipelineTests));
                scanner.IncludeNamespaceContainingType<Cing>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var compiledPipeline = new CompiledPipeline<Cing, Cong>(mediator, container);
        
        // Act
        compiledPipeline.RegisterHandler<Cing, Cong>();
        var result = compiledPipeline.Prepare<Cing, Cong>();

        // Assert
        result.ShouldNotBeNull();
        
        var item = (CompiledPipelineItem<Cing, Cong>?)compiledPipeline.QueryItems().FirstOrDefault();
        item.ShouldNotBeNull();
        item.PreparedHandler.ShouldNotBeNull();

        compiledPipeline.QueryItems().Count().ShouldBe(1);
        
        CompiledPipelineItem<Cing, Cong> handlerItem = (CompiledPipelineItem<Cing,Cong>)compiledPipeline.QueryItems().First();
        handlerItem.ItemType.ShouldBe(CompiledPipelineItem.ItemTypes.Handler);

        var handler = handlerItem.AsHandlerExpression();
        handler.ShouldNotBeNull();

        var f = handler.Compile();
        var res = f(new Cing { Message = " me " }, CancellationToken.None);

        res.ShouldNotBeNull();
        res.Result.ShouldNotBeNull();
        res.Result.Message.ShouldBe(" me  cinging ");
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
                scanner.IncludeNamespaceContainingType<Cing>();
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
        var result = await mediator.Send(new Cing { Message = " me " });
        
        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBe(" me  I'm  cinging in the rain!"); 
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
                scanner.IncludeNamespaceContainingType<Cing>();
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

        var pipeline = new CompiledPipeline<Cing, Cong>(mediator, container);

        // Act
        pipeline.RegisterPreProcessor<Cing, Cong>();
        pipeline.RegisterHandler<Cing, Cong>();
        pipeline.RegisterPostProcessor<Cing, Cong>();

        var compiledPipeline = pipeline.Prepare<Cing, Cong>().Compile<Cing, Cong>();

        // Assert
        compiledPipeline.ShouldNotBeNull();

        var cing = new Cing { Message = " me " };
        var cong = new Cong();
        var cancellationTokenSource = new CancellationTokenSource();

        var res= await compiledPipeline(cing, cancellationTokenSource.Token);
        
        res.ShouldNotBeNull();
        res.Message.ShouldNotBeNull();
        res.Message.ShouldBe(" me  I'm  cinging in the rain!");
    }
}
