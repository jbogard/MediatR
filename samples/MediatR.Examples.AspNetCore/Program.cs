using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.Abstraction.Behaviors;
using MediatR.Abstraction.Handlers;
using MediatR.DependencyInjection.ConfigurationBase;
using MediatR.Examples.Streams;
using MediatR.MicrosoftDiCExtensions;
using Microsoft.Extensions.DependencyInjection;


namespace MediatR.Examples.AspNetCore;

public static class Program
{
    public static Task Main(string[] args)
    {
        var writer = new WrappingWriter(Console.Out);
        var mediator = BuildMediator(writer);
        return Runner.Run(mediator, writer, "ASP.NET Core DI", testStreams: true);
    }

    private static IMediator BuildMediator(WrappingWriter writer)
    {
        var services = new ServiceCollection();

        services.AddSingleton<TextWriter>(writer);
        services.AddSingleton<IStreamRequestHandler<Sing, Song>, SingHandler>();

        services.AddMediatR(cfg =>
        {
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly, typeof(Sing).Assembly);
        });

        foreach (var service in services)
        {
            if (service.ServiceType == typeof(IPipelineBehavior<>))
            {
                Console.WriteLine(service.ImplementationType);
            }
        }

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IMediator>();
    }
}