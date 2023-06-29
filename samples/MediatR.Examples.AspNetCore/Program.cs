using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.MicrosoftDICExtensions;
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

        services.ConfigureMediatR(cfg =>
        {
            cfg.RequestExceptionActionProcessorStrategy = RequestExceptionActionProcessorStrategy.ApplyForAllExceptions;
            cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly, typeof(Sing).Assembly);
        });

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IMediator>();
    }
}