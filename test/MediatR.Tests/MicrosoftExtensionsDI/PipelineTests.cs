using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pipeline;
using Shouldly;
using Xunit;

public class PipelineTests
{
    public class OuterBehavior : IPipelineBehavior<Ping, Pong>
    {
        private readonly Logger _output;

        public OuterBehavior(Logger output)
        {
            _output = output;
        }

        public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Outer before");
            var response = await next();
            _output.Messages.Add("Outer after");

            return response;
        }
    }

    public class InnerBehavior : IPipelineBehavior<Ping, Pong>
    {
        private readonly Logger _output;

        public InnerBehavior(Logger output)
        {
            _output = output;
        }

        public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Inner before");
            var response = await next();
            _output.Messages.Add("Inner after");

            return response;
        }
    }

    public class InnerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly Logger _output;

        public InnerBehavior(Logger output)
        {
            _output = output;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Inner generic before");
            var response = await next();
            _output.Messages.Add("Inner generic after");

            return response;
        }
    }

    public class OuterBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly Logger _output;

        public OuterBehavior(Logger output)
        {
            _output = output;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Outer generic before");
            var response = await next();
            _output.Messages.Add("Outer generic after");

            return response;
        }
    }

    public class ConstrainedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Pong
    {
        private readonly Logger _output;

        public ConstrainedBehavior(Logger output)
        {
            _output = output;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Constrained before");
            var response = await next();
            _output.Messages.Add("Constrained after");

            return response;
        }
    }

    public class FirstPreProcessor<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    {
        private readonly Logger _output;

        public FirstPreProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            _output.Messages.Add("First pre processor");
            return Task.FromResult(0);
        }
    }

    public class FirstConcretePreProcessor : IRequestPreProcessor<Ping>
    {
        private readonly Logger _output;

        public FirstConcretePreProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(Ping request, CancellationToken cancellationToken)
        {
            _output.Messages.Add("First concrete pre processor");
            return Task.FromResult(0);
        }
    }

    public class NextPreProcessor<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    {
        private readonly Logger _output;

        public NextPreProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Next pre processor");
            return Task.FromResult(0);
        }
    }

    public class NextConcretePreProcessor : IRequestPreProcessor<Ping>
    {
        private readonly Logger _output;

        public NextConcretePreProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(Ping request, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Next concrete pre processor");
            return Task.FromResult(0);
        }
    }

    public class FirstPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly Logger _output;

        public FirstPostProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            _output.Messages.Add("First post processor");
            return Task.FromResult(0);
        }
    }

    public class FirstConcretePostProcessor : IRequestPostProcessor<Ping, Pong>
    {
        private readonly Logger _output;

        public FirstConcretePostProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
        {
            _output.Messages.Add("First concrete post processor");
            return Task.FromResult(0);
        }
    }

    public class NextPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly Logger _output;

        public NextPostProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Next post processor");
            return Task.FromResult(0);
        }
    }

    public class NextConcretePostProcessor : IRequestPostProcessor<Ping, Pong>
    {
        private readonly Logger _output;

        public NextConcretePostProcessor(Logger output)
        {
            _output = output;
        }
        public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Next concrete post processor");
            return Task.FromResult(0);
        }
    }

    public class PingPongGenericExceptionAction : IRequestExceptionAction<Ping>
    {
        private readonly Logger _output;

        public PingPongGenericExceptionAction(Logger output) => _output = output;

        public Task Execute(Ping request, Exception exception, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Logging generic exception");

            return Task.CompletedTask;
        }
    }

    public class PingPongApplicationExceptionAction : IRequestExceptionAction<Ping, ApplicationException>
    {
        private readonly Logger _output;

        public PingPongApplicationExceptionAction(Logger output) => _output = output;

        public Task Execute(Ping request, ApplicationException exception, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Logging ApplicationException exception");

            return Task.CompletedTask;
        }
    }

    public class PingPongExceptionActionForType1 : IRequestExceptionAction<Ping, SystemException>
    {
        private readonly Logger _output;

        public PingPongExceptionActionForType1(Logger output) => _output = output;

        public Task Execute(Ping request, SystemException exception, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Logging exception 1");

            return Task.CompletedTask;
        }
    }

    public class PingPongExceptionActionForType2 : IRequestExceptionAction<Ping, SystemException>
    {
        private readonly Logger _output;

        public PingPongExceptionActionForType2(Logger output) => _output = output;

        public Task Execute(Ping request, SystemException exception, CancellationToken cancellationToken)
        {
            _output.Messages.Add("Logging exception 2");

            return Task.CompletedTask;
        }
    }

    public class PingPongExceptionHandlerForType : IRequestExceptionHandler<Ping, Pong, ApplicationException>
    {
        public Task Handle(Ping request, ApplicationException exception, RequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
        {
            state.SetHandled(new Pong { Message = exception.Message + " Handled by Specific Type" });

            return Task.CompletedTask;
        }
    }

    public class PingPongGenericExceptionHandler : IRequestExceptionHandler<Ping, Pong>
    {
        private readonly Logger _output;

        public PingPongGenericExceptionHandler(Logger output) => _output = output;

        public Task Handle(Ping request, Exception exception, RequestExceptionHandlerState<Pong> state, CancellationToken cancellationToken)
        {
            _output.Messages.Add(exception.Message + " Logged by Generic Type");

            return Task.CompletedTask;
        }
    }

    public class NotAnOpenBehavior : IPipelineBehavior<Ping, Pong>
    {
        public Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken) => next();
    }

    public class NotAnOpenStreamBehavior : IStreamPipelineBehavior<Ping, Pong>
    {
        public IAsyncEnumerable<Pong> Handle(Ping request, StreamHandlerDelegate<Pong> next, CancellationToken cancellationToken) => next();
    }

    public class OpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) => next();
    }

    public class OpenStreamBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken) => next();
    }

    public class MultiOpenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, IStreamPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) => next();

        public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken) => next();
    }

    [Fact]
    public async Task Should_wrap_with_behavior()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddTransient<IPipelineBehavior<Ping, Pong>, OuterBehavior>();
        services.AddTransient<IPipelineBehavior<Ping, Pong>, InnerBehavior>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");

        output.Messages.ShouldBe(new[]
        {
            "Outer before",
            "Inner before",
            "First concrete pre processor",
            "Next concrete pre processor",
            "First pre processor",
            "Next pre processor",
            "Handler",
            "First concrete post processor",
            "Next concrete post processor",
            "First post processor",
            "Next post processor",
            "Inner after",
            "Outer after"
        });
    }

    [Fact]
    public async Task Should_wrap_generics_with_behavior()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddMediatR(cfg =>
        {
            // Call these registration methods multiple times to prove we don't register a service if it is already registered
            for (var i = 0; i < 3; i++)
            {
                cfg.AddOpenBehavior(typeof(OuterBehavior<,>));
                cfg.AddOpenBehavior(typeof(InnerBehavior<,>));
                cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
            }
        });
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
            
        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");

        output.Messages.ShouldBe(new[]
        {
            "Outer generic before",
            "Inner generic before",
            "First concrete pre processor",
            "Next concrete pre processor",
            "First pre processor",
            "Next pre processor",
            "Handler",
            "First concrete post processor",
            "Next concrete post processor",
            "First post processor",
            "Next post processor",
            "Inner generic after",
            "Outer generic after",
        });
    }

    [Fact]
    public async Task Should_pick_up_pre_and_post_processors()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");

        output.Messages.ShouldBe(new[]
        {
            "First concrete pre processor",
            "Next concrete pre processor",
            "First pre processor",
            "Next pre processor",
            "Handler",
            "First concrete post processor",
            "Next concrete post processor",
            "First post processor",
            "Next post processor",
        });
    }

    [Fact]
    public async Task Should_pick_up_specific_exception_behaviors()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new Ping {Message = "Ping", ThrowAction = msg => throw new ApplicationException(msg.Message + " Thrown")});

        response.Message.ShouldBe("Ping Thrown Handled by Specific Type");
        output.Messages.ShouldNotContain("Logging ApplicationException exception");
    }

    [Fact]
    public void Should_pick_up_base_exception_behaviors()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        Should.Throw<Exception>(async () => await mediator.Send(new Ping {Message = "Ping", ThrowAction = msg => throw new Exception(msg.Message + " Thrown")}));

        output.Messages.ShouldContain("Ping Thrown Logged by Generic Type");
        output.Messages.ShouldContain("Logging generic exception");
    }

    [Fact]
    public void Should_pick_up_exception_actions()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        Should.Throw<SystemException>(async () => await mediator.Send(new Ping {Message = "Ping", ThrowAction = msg => throw new SystemException(msg.Message + " Thrown")}));

        output.Messages.ShouldContain("Logging exception 1");
        output.Messages.ShouldContain("Logging exception 2");
    }

    [Fact]
    public async Task Should_handle_constrained_generics()
    {
        var output = new Logger();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(output);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InnerBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConstrainedBehavior<,>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new Ping { Message = "Ping" });

        response.Message.ShouldBe("Ping Pong");

        output.Messages.ShouldBe(new[]
        {
            "Outer generic before",
            "Inner generic before",
            "Constrained before",
            "First concrete pre processor",
            "Next concrete pre processor",
            "First pre processor",
            "Next pre processor",
            "Handler",
            "First concrete post processor",
            "Next concrete post processor",
            "First post processor",
            "Next post processor",
            "Constrained after",
            "Inner generic after",
            "Outer generic after"
        });

        output.Messages.Clear();

        var zingResponse = await mediator.Send(new Zing { Message = "Zing" });

        zingResponse.Message.ShouldBe("Zing Zong");

        output.Messages.ShouldBe(new[]
        {
            "Outer generic before",
            "Inner generic before",
            "First pre processor",
            "Next pre processor",
            "Handler",
            "First post processor",
            "Next post processor",
            "Inner generic after",
            "Outer generic after"
        });
    }

    [Fact]
    public void Should_throw_when_adding_non_open_behavior()
    {
        Should.Throw<InvalidOperationException>(() => new MediatRServiceConfiguration().AddOpenBehavior(typeof(NotAnOpenBehavior)));
    }

    [Fact]
    public void Should_throw_when_adding_non_open_stream_behavior()
    {
        Should.Throw<InvalidOperationException>(() => new MediatRServiceConfiguration().AddOpenBehavior(typeof(NotAnOpenStreamBehavior)));
    }

    [Fact]
    public void Should_throw_when_adding_random_generic_type_as_open_behavior()
    {
        Should.Throw<InvalidOperationException>(() => new MediatRServiceConfiguration().AddOpenBehavior(typeof(List<string>)));
    }

    [Fact]
    public void Should_handle_open_behavior_registration()
    {
        var cfg = new MediatRServiceConfiguration();
        cfg.AddOpenBehavior(typeof(OpenBehavior<,>));
        cfg.AddOpenBehavior(typeof(OpenStreamBehavior<,>));

        cfg.BehaviorsToRegister.Count.ShouldBe(2);

        cfg.BehaviorsToRegister[0].ServiceType.ShouldBe(typeof(IPipelineBehavior<,>));
        cfg.BehaviorsToRegister[0].ImplementationType.ShouldBe(typeof(OpenBehavior<,>));
        cfg.BehaviorsToRegister[0].ImplementationFactory.ShouldBeNull();
        cfg.BehaviorsToRegister[0].ImplementationInstance.ShouldBeNull();
        cfg.BehaviorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Transient);

        cfg.BehaviorsToRegister[1].ServiceType.ShouldBe(typeof(IStreamPipelineBehavior<,>));
        cfg.BehaviorsToRegister[1].ImplementationType.ShouldBe(typeof(OpenStreamBehavior<,>));
        cfg.BehaviorsToRegister[1].ImplementationFactory.ShouldBeNull();
        cfg.BehaviorsToRegister[1].ImplementationInstance.ShouldBeNull();
        cfg.BehaviorsToRegister[1].Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void Should_handle_open_behaviors_registration_from_a_single_type()
    {
        var cfg = new MediatRServiceConfiguration();
        cfg.AddOpenBehavior(typeof(MultiOpenBehavior<,>), ServiceLifetime.Singleton);

        cfg.BehaviorsToRegister.Count.ShouldBe(2);

        cfg.BehaviorsToRegister[0].ServiceType.ShouldBe(typeof(IPipelineBehavior<,>));
        cfg.BehaviorsToRegister[0].ImplementationType.ShouldBe(typeof(MultiOpenBehavior<,>));
        cfg.BehaviorsToRegister[0].ImplementationFactory.ShouldBeNull();
        cfg.BehaviorsToRegister[0].ImplementationInstance.ShouldBeNull();
        cfg.BehaviorsToRegister[0].Lifetime.ShouldBe(ServiceLifetime.Singleton);

        cfg.BehaviorsToRegister[1].ServiceType.ShouldBe(typeof(IStreamPipelineBehavior<,>));
        cfg.BehaviorsToRegister[1].ImplementationType.ShouldBe(typeof(MultiOpenBehavior<,>));
        cfg.BehaviorsToRegister[1].ImplementationFactory.ShouldBeNull();
        cfg.BehaviorsToRegister[1].ImplementationInstance.ShouldBeNull();
        cfg.BehaviorsToRegister[1].Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }
}