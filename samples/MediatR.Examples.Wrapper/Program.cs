using System;
using System.IO;
using System.Threading.Tasks;
using MediatR.Examples.Wrapper.Commands;
using MediatR.Examples.Wrapper.Core;
using MediatR.Examples.Wrapper.Queries;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.Wrapper
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            await mediator.Handle(new JingCommand {Message = "Hello world"});

            var response = await mediator.Handle<PingQuery, PongResponse>(new PingQuery {Message = "Hello world"});

            await writer.WriteLineAsync($"--- Handled PingQuery with response: {response.Message}");
        }

        private static IMediate BuildMediator(WrappingWriter writer)
        {
            var services = new ServiceCollection();

            //Register wrappers
            services.AddTransient<IMediate, Mediate>();
            services.AddTransient<ICommandMediator, Mediate>();
            services.AddTransient<IQueryMediator, Mediate>();

            services.AddScoped<ServiceFactory>(p => p.GetService);

            services.AddSingleton<TextWriter>(writer);

            //Pipeline
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));

            //This causes a type load exception. https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/issues/12
            //services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>));
            //services.AddScoped(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>));

            // Use Scrutor to scan and register all
            // classes as their implemented interfaces.
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IMediator), typeof(IMediate))
                .AddClasses()
                .AsImplementedInterfaces());

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediate>();
        }
    }
}
