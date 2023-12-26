using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Shouldly;
using Lamar;
using MediatR.Internal;
using Xunit;

namespace MediatR.Tests.Pipeline.Ordering;

public class OrderedProcessorTests
{
    public class MyRequest : IRequest<MyResponse>
    {
        public string Message { get; set; } = "initial";
    }

    public class MyResponse
    {
        public string? Message { get; set; }
    }

    public class DefaultHandler : IRequestHandler<MyRequest, MyResponse>
    {
        public Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken) => 
            Task.FromResult(new MyResponse { Message = request.Message + " handler" });
    }

    public class DefaultPreHandler : IRequestPreProcessor<MyRequest>
    {
        public Task Process(MyRequest request, CancellationToken cancellationToken)
        {
            request.Message += " default-prehandler";
            return Task.CompletedTask;
        }
    }

    public class FirstPreHandler : IRequestPreProcessor<MyRequest>
    {
        public int Order => -100;

        public Task Process(MyRequest request, CancellationToken cancellationToken)
        {
            request.Message += " first-prehandler";
            return Task.CompletedTask;
        }
    }

    public class LastPreHandler : IRequestPreProcessor<MyRequest>, IOrderableHandler
    {
        public int Order => 100;

        public Task Process(MyRequest request, CancellationToken cancellationToken)
        {
            request.Message += " last-prehandler";
            return Task.CompletedTask;
        }
    }

    public class DefaultPostHandler : IRequestPostProcessor<MyRequest, MyResponse>
    {
        public Task Process(MyRequest request, MyResponse response, CancellationToken cancellationToken)
        {
            response.Message += " default-posthandler";
            return Task.CompletedTask;
        }
    }

    public class FirstPostHandler : IRequestPostProcessor<MyRequest, MyResponse>
    {
        public int Order => -100;

        public Task Process(MyRequest request, MyResponse response, CancellationToken cancellationToken)
        {
            response.Message += " first-posthandler";
            return Task.CompletedTask;
        }
    }

    public class LastPostHandler : IRequestPostProcessor<MyRequest, MyResponse>, IOrderableHandler
    {
        public int Order => 100;

        public Task Process(MyRequest request, MyResponse response, CancellationToken cancellationToken)
        {
            response.Message += " last-posthandler";
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_run_preprocessors()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(DefaultHandler));
                scanner.IncludeNamespaceContainingType<MyRequest>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestPreProcessor<>));
                scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
            });
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPreProcessorBehavior<,>));
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var response = await mediator.Send(new MyRequest());

        // proper execution order is:
        //
        // initial  (set on the request)
        // first-prehandler due to magic -100 Order sorting it first
        // default-prehandler due to 0 Order (default)
        // last-prehandler due to +100 Order from IOrderableHandler
        // handler from the IRequestHandler itself
        // first-posthandler due to magic -100 Order sorting it first
        // default-posthandler due to 0 Order (default)
        // last-posthandler due to +100 Order from IOrderableHandler
        response.Message.ShouldBe("initial first-prehandler default-prehandler last-prehandler handler first-posthandler default-posthandler last-posthandler");
    }

    [Fact]
    public void Order_is_discoverable()
    {
        (new DefaultPreHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(0);
        (new FirstPreHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(-100);
        (new LastPreHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(100);

        (new DefaultPostHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(0);
        (new FirstPostHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(-100);
        (new LastPostHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(100);
    }
}
