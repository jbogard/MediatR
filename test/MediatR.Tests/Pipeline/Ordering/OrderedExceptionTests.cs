using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lamar;
using MediatR.Internal;
using MediatR.Pipeline;
using Shouldly;
using Xunit;

namespace MediatR.Tests.Pipeline.Ordering;
public class OrderedExceptionTests
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
            throw new InvalidOperationException("this should break");
    }

    public class DefaultExceptionHandler : IRequestExceptionHandler<MyRequest, MyResponse, InvalidOperationException>
    {
        public Task Handle(MyRequest request, InvalidOperationException exception, RequestExceptionHandlerState<MyResponse> state,
            CancellationToken cancellationToken)
        {
            state.SetHandled(new MyResponse { Message = "default-exception" });
            return Task.CompletedTask;
        }
    }

    public class FirstExceptionHandler : IRequestExceptionHandler<MyRequest, MyResponse, InvalidOperationException>
    {
        public int Order => -100;

        public Task Handle(MyRequest request, InvalidOperationException exception, RequestExceptionHandlerState<MyResponse> state,
            CancellationToken cancellationToken)
        {
            state.SetHandled(new MyResponse { Message = "first-exception" });
            return Task.CompletedTask;
        }
    }


    [Fact]
    public async Task Should_run_exceptionprocessors()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(DefaultHandler));
                scanner.IncludeNamespaceContainingType<MyRequest>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestExceptionHandler<,,>));
            });
            cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestExceptionProcessorBehavior<,>));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var response = await mediator.Send(new MyRequest());

        // the exception handler just resets the message, so this should be the correct one
        response.Message.ShouldBe("first-exception");
    }

    [Fact]
    public void Order_is_discoverable()
    {
        (new DefaultExceptionHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(0);
        (new FirstExceptionHandler()).GetOrderIfExists().GetValueOrDefault(0).ShouldBe(-100);
    }
}
