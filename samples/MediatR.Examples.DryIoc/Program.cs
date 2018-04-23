using System;
using System.IO;
using System.Threading.Tasks;
using DryIoc;

namespace MediatR.Examples.DryIoc
{
    class Program
    {
        static Task Main()
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "DryIoc");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new Container();

            container.UseInstance<TextWriter>(writer);
            container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), typeof(Ping).GetAssembly() }, Registrator.Interfaces);

            container.Register<IMediator, InjectingMediator>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.UseInstance<ServiceFactory>(container.Resolve);

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
