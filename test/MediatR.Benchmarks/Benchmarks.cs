using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using MediatR.Examples;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Benchmarks
{
    [DotTraceDiagnoser]
    public class Benchmarks
    {
        private IMediator _mediator;
        private readonly Ping _request = new Ping {Message = "Hello World"};
        private readonly Pinged _notification = new Pinged();

        [GlobalSetup]
        public void GlobalSetup()
        {
            var services = new ServiceCollection();

            services.AddSingleton(TextWriter.Null);

            services.AddMediatR(typeof(Ping));

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

            var provider = services.BuildServiceProvider();

            _mediator = provider.GetRequiredService<IMediator>();
        }

        [Benchmark]
        public Task SendingRequests()
        {
            return _mediator.SendAsync(_request);
        }

        [Benchmark]
        public Task PublishingNotifications()
        {
            return _mediator.PublishAsync(_notification);
        }
    }
}
