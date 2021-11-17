using System.Reflection;
using System.Threading;

namespace MediatR.Tests
{
    using System;
    using System.Linq;
    using Shouldly;
    using StructureMap;
    using System.Threading.Tasks;
    using Xunit;

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

        public class Ping : IRequest<Pong>, IPingPong
        {
            public string Message { get; set; }
        }

        public class Pong : IPingPong
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
                cfg.For<IGenericRequest<Ping, Pong>>().Use<GenericRequest>();
                cfg.For<IRequestHandler<IGenericRequest<Ping, Pong>, Pong>>().Use<GenericHandler<Ping, Pong>>();
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

            // Assert it is of type IRequest and IRequest<T>
            Assert.True(genericTypeConstraintsVoidReturn.IsIRequest);
            Assert.True(genericTypeConstraintsVoidReturn.IsIRequestT);
            Assert.True(genericTypeConstraintsVoidReturn.IsIBaseRequest);

            // Verify it is of IRequest and IBaseRequest and IRequest<Unit>
            var results = genericTypeConstraintsVoidReturn.Handle(jing);

            Assert.Equal(3, results.Length);

            results.ShouldContain(typeof(IRequest<Unit>));
            results.ShouldContain(typeof(IBaseRequest));
            results.ShouldContain(typeof(IRequest));
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

            // Assert it is of type IRequest<T> but not IRequest
            Assert.False(genericTypeConstraintsResponseReturn.IsIRequest);
            Assert.True(genericTypeConstraintsResponseReturn.IsIRequestT);
            Assert.True(genericTypeConstraintsResponseReturn.IsIBaseRequest);

            // Verify it is of IRequest<Pong> and IBaseRequest, but not IRequest
            var results = genericTypeConstraintsResponseReturn.Handle(ping);

            Assert.Equal(2, results.Length);

            results.ShouldContain(typeof(IRequest<Pong>));
            results.ShouldContain(typeof(IBaseRequest));
            results.ShouldNotContain(typeof(IRequest));
        }

        public class GenericRequest : IGenericRequest<Ping, Pong>
        {
            public Ping Payload { get; set; }
        }

        public interface IPingPong { 
            string Message { get; set; }    
        }

        public interface IGenericRequest<TRequestType, TResponseType> : IRequest<TResponseType>
            where TRequestType : IPingPong
            where TResponseType : IPingPong
        {
            TRequestType Payload { get; set; }
        }

        public class GenericHandler<TRequestType, TResponseType> :
            IRequestHandler<IGenericRequest<TRequestType, TResponseType>, TResponseType>
            where TRequestType : IPingPong
            where TResponseType : IPingPong
        {
            private readonly ServiceFactory _serviceFactory;

            public GenericHandler(ServiceFactory serviceFactory)
            {
                _serviceFactory = serviceFactory;
            }

            public Task<TResponseType> Handle(IGenericRequest<TRequestType, TResponseType> request, CancellationToken cancellationToken)
            {
                var response = (TResponseType) _serviceFactory(typeof(TResponseType));
                response.Message = request.Payload.Message + " Pong";
                return Task.FromResult(response);
                //return Task.FromResult(new Pong { Message = request.Payload.Message + " Pong" });
            }
        }

        [Fact]
        public async Task Should_Not_Resolve_Multiple_GenericTypes_Handler()
        {
            // Create Request
            var request = new GenericRequest { Payload = new Ping { Message = "Ping" } };

            // Test will fail as the IOC container can not find implementation of the handler which is a
            // interface based generic class, this works on some containers such as Lamar, but did not
            // work on StructureMap or Castle.Windsor
            await Assert.ThrowsAsync<InvalidOperationException>(async () => { await _mediator.Send(request); });
        }

        [Fact]
        public async Task Should_Resolve_Multiple_GenericTypes_Handler()
        {
            // Create Request
            var request = new GenericRequest { Payload = new Ping { Message = "Ping" } };

            // Test mediator still works sending request and gets response when told
            // what the interface for the request is
            var response = await _mediator.Send<IGenericRequest<Ping, Pong>, Pong>(request);

            response.Message.ShouldBe("Ping Pong");

            // Create new instance of type constrained class
            var genericTypeConstraintsResponseReturn = new GenericTypeConstraintPing();

            // Assert it is of type IRequest<T> but not IRequest
            Assert.False(genericTypeConstraintsResponseReturn.IsIRequest);
            Assert.True(genericTypeConstraintsResponseReturn.IsIRequestT);
            Assert.True(genericTypeConstraintsResponseReturn.IsIBaseRequest);

            // Verify it is of IRequest<Pong> and IBaseRequest, but not IRequest
            var results = genericTypeConstraintsResponseReturn.Handle(request.Payload);

            Assert.Equal(3, results.Length);

            results.ShouldContain(typeof(IRequest<Pong>));
            results.ShouldContain(typeof(IBaseRequest));
            results.ShouldNotContain(typeof(IRequest));
        }
    }
}
