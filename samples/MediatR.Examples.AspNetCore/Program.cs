using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;


namespace MediatR.Examples.AspNetCore
{
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

#if !NETCOREAPP3_1_OR_GREATER
            services.AddMediatR(typeof(Ping));
#else
            services.AddMediatR(typeof(Ping), typeof(Sing));

            services.AddScoped(typeof(IStreamRequestHandler<Sing, Song>), typeof(SingHandler));
#endif

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

#if NETCOREAPP3_1_OR_GREATER
            services.AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamPipelineBehavior<,>));
#endif
            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }
    }
}
