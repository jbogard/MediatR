using System;
using System.IO;
using System.Threading.Tasks;
using DryIocZero;

namespace MediatR.Examples.DryIocZero
{
    static class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);
            return Runner.Run(mediator, writer, "DryIocZero");
        }

        private static IMediator BuildMediator(TextWriter writer)
        {
            var container = new Container();

            container.UseInstance(writer);
            container.UseInstance<IMediator>(new InjectingMediator(container.Resolve));

            return container.Resolve<IMediator>();
        }

        class InjectingMediator : IMediator
        {
            private readonly ServiceFactory _factory;

            public InjectingMediator(ServiceFactory factory) => _factory = factory;

            public IRequestMediator<TResponse> GetRequestMediator<TResponse>(Type requestType) =>
                (IRequestMediator<TResponse>)_factory(typeof(IRequestMediator<,>).MakeGenericType(requestType, typeof(TResponse)));

            public INotificationMediator<TNotification> GetNotificationMediator<TNotification>()
                where TNotification : INotification =>
                (INotificationMediator<TNotification>)_factory(typeof(INotificationMediator<>).MakeGenericType(typeof(TNotification)));
        }
    }
}
