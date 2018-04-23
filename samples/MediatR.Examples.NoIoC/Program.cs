using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Examples.DryIoc
{
    class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);

            var mediator = new InjectingMediator(serviceType => Bootstrap(serviceType, writer));

            return Runner.Run(mediator, writer, "DryIoc");
        }

        class InjectingMediator : IMediator
        {
            private readonly ServiceFactory _factory;

            public InjectingMediator(ServiceFactory factory) => _factory = factory;

            public IRequestMediator<TResponse> GetRequestMediator<TResponse>(Type requestType) =>
                (IRequestMediator<TResponse>)_factory(typeof(IRequestMediator<,>).MakeGenericType(requestType, typeof(TResponse)));

            public INotificationMediator<TNotification> GetNotificationMediator<TNotification>() where TNotification : INotification =>
                (INotificationMediator<TNotification>)_factory(typeof(INotificationMediator<>).MakeGenericType(typeof(TNotification)));
        }

        private static object Bootstrap(Type serviceType, TextWriter writer)
        {
            if (serviceType == typeof(IRequestMediator<Ping, Pong>))
                return GetPingPongMediator(writer);

            if (serviceType == typeof(IRequestMediator<Jing, Unit>))
                return GetJingUnitRequest(writer);

            if (serviceType == typeof(INotificationMediator<Pinged>))
                return GetPingedMediator(writer);

            if (serviceType == typeof(INotificationMediator<Ponged>))
                return GetPongedMediator(writer);

            throw new ArgumentException($"Not supported {serviceType}", nameof(serviceType));
        }

        private static object GetPingPongMediator(TextWriter writer)
        {
            return new RequestMediator<Ping, Pong>(
                new PingHandler(writer),
                new IPipelineBehavior<Ping, Pong>[]
                {
                    new RequestPreProcessorBehavior<Ping, Pong>(
                        new IRequestPreProcessor<Ping>[]
                        {
                            new GenericRequestPreProcessor<Ping>(writer)
                        }),
                    new RequestPostProcessorBehavior<Ping, Pong>(
                        new IRequestPostProcessor<Ping, Pong>[]
                        {
                            new ConstrainedRequestPostProcessor<Ping, Pong>(writer),
                            new GenericRequestPostProcessor<Ping, Pong>(writer)
                        }),
                    new GenericPipelineBehavior<Ping, Pong>(writer)
                });
        }

        private static RequestMediator<Jing, Unit> GetJingUnitRequest(TextWriter writer)
        {
            return new RequestMediator<Jing, Unit>(
                new JingHandler(writer),
                new IPipelineBehavior<Jing, Unit>[]
                {
                    new RequestPreProcessorBehavior<Jing, Unit>(
                        new IRequestPreProcessor<Jing>[]
                        {
                            new GenericRequestPreProcessor<Jing>(writer)
                        }),
                    new RequestPostProcessorBehavior<Jing, Unit>(
                        new IRequestPostProcessor<Jing, Unit>[]
                        {
                            new GenericRequestPostProcessor<Jing, Unit>(writer)
                        }),
                    new GenericPipelineBehavior<Jing, Unit>(writer)
                });
        }

        private static object GetPingedMediator(TextWriter writer)
        {
            return new NotificationMediator<Pinged>(
                new INotificationHandler<Pinged>[]
                {
                    new GenericHandler(writer),
                    new PingedHandler(writer),
                    new ConstrainedPingedHandler<Pinged>(writer),
                    new PingedAlsoHandler(writer)
                });
        }

        private static NotificationMediator<Ponged> GetPongedMediator(TextWriter writer)
        {
            return new NotificationMediator<Ponged>(
                new INotificationHandler<Ponged>[]
                {
                    new GenericHandler(writer),
                    new PongedHandler(writer)
                });
        }
    }
}
