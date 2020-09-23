using System.Reflection;
using System.Threading;
using VerifyXunit;

namespace MediatR.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using StructureMap;
    using System.Threading.Tasks;
    using Xunit;

    [UsesVerify]
    public class GenericTypeConstraintsTests
    {
        public interface IGenericTypeRequestHandlerTestClass<TRequest> where TRequest : IBaseRequest
        {
            Type[] Handle(TRequest request);
        }

        public abstract class GenericTypeRequestHandlerTestClass<TRequest> : IGenericTypeRequestHandlerTestClass<TRequest>
            where TRequest : IBaseRequest
        {
            public bool IsIRequest { get; }


            public bool IsIRequestT { get; }

            public bool IsIBaseRequest { get; }

            public GenericTypeRequestHandlerTestClass()
            {
                IsIRequest = typeof(IRequest).IsAssignableFrom(typeof(TRequest));
                IsIRequestT = typeof(TRequest).GetInterfaces()
                                                   .Any(x => x.GetTypeInfo().IsGenericType &&
                                                             x.GetGenericTypeDefinition() == typeof(IRequest<>));

                IsIBaseRequest = typeof(IBaseRequest).IsAssignableFrom(typeof(TRequest));
            }

            public Type[] Handle(TRequest request)
            {
                return typeof(TRequest).GetInterfaces();
            }
        }

        public class GenericTypeConstraintPing : GenericTypeRequestHandlerTestClass<Ping>
        {

        }

        public class GenericTypeConstraintJing : GenericTypeRequestHandlerTestClass<Jing>
        {

        }

        public class Jing : IRequest
        {
            public string Message { get; set; }
        }

        public class JingHandler : IRequestHandler<Jing, Unit>
        {
            public Task<Unit> Handle(Jing request, CancellationToken cancellationToken)
            {
                // empty handle
                return Unit.Task;
            }
        }

        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IRequestHandler<Ping, Pong>
        {
            public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Pong { Message = request.Message + " Pong" });
            }
        }

        private readonly IMediator _mediator;

        public GenericTypeConstraintsTests()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(GenericTypeConstraintsTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.IncludeNamespaceContainingType<Jing>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                });
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);
                cfg.For<IMediator>().Use<Mediator>();
            });

            _mediator = container.GetInstance<IMediator>();
        }

        [Fact]
        public async Task Should_Resolve_Void_Return_Request()
        {
            // Create Request
            var jing = new Jing { Message = "Jing" };

            // Test mediator still works sending request
            await _mediator.Send(jing);

            // Create new instance of type constrained class
            var genericTypeConstraintsVoidReturn = new  GenericTypeConstraintJing();

            var results = genericTypeConstraintsVoidReturn.Handle(jing);

            await Verifier.Verify(
                new
                {
                    genericTypeConstraintsVoidReturn,
                    results
                });
        }

        [Fact]
        public async Task Should_Resolve_Response_Return_Request()
        {
            // Create Request
            var ping = new Ping { Message = "Ping" };

            // Test mediator still works sending request and gets response
            var pingResponse = await _mediator.Send(ping);
            pingResponse.Message.ShouldBe("Ping Pong");

            // Create new instance of type constrained class
            var genericTypeConstraintsResponseReturn = new GenericTypeConstraintPing();

            var results = genericTypeConstraintsResponseReturn.Handle(ping);

            await Verifier.Verify(
                new
                {
                    genericTypeConstraintsResponseReturn,
                    results
                });
        }
    }
}
