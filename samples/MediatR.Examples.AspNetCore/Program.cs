using System;
using System.Collections.Generic;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace MediatR.Examples.AspNetCore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var mediator = BuildMediator();
            Runner.Run(mediator, Console.Out).Wait();
            Console.ReadKey();
        }

        private static IMediator BuildMediator()
        {
            var services = new ServiceCollection();

            services.AddScoped<SingleInstanceFactory>(p => t => p.GetRequiredService(t));
            services.AddScoped<MultiInstanceFactory>(p => t => p.GetRequiredServices(t));

            services.AddSingleton(Console.Out);

            //Pipeline
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

            // Use Scrutor to scan and register all
            // classes as their implemented interfaces.
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IMediator), typeof(Ping))
                .AddClasses()
                .AsImplementedInterfaces());

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }

        private static IEnumerable<object> GetRequiredServices(this IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>) provider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(serviceType));
        } 
    }
}
