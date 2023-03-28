using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Benchmarks
{
    [DotTraceDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class Benchmarks
    {
        private IMediator _mediator;
       
        private readonly Ping _request = new Ping {Message = "Hello World"};
        private readonly Pinged _notification = new Pinged();

        private CompiledPipeline.CompiledPipeline<Cing, Cong> _compiledPipeline;
        private readonly Cing _cequest = new Cing {Message = "Hello World: "};
        private readonly Cong _cesponse = new Cong();
        
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            var services = new ServiceCollection();

            services.AddSingleton(TextWriter.Null);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(Ping)));

            services.AddScoped(typeof(IRequestHandler<Cing,Cong>), typeof(CingRequestHandler));
            services.AddScoped(typeof(IRequestPreProcessor<Cing>), typeof(CingPreProcessor));
            services.AddScoped(typeof(IRequestPostProcessor<Cing,Cong>), typeof(CingPostProcessor));
            
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));
            

            var provider = services.BuildServiceProvider();

            _mediator = provider.GetRequiredService<IMediator>();
            
            _compiledPipeline = new CompiledPipeline.CompiledPipeline<Cing, Cong>(_mediator, provider);
        
            _compiledPipeline.RegisterHandler<Cing, Cong>();
            _compiledPipeline
                .Prepare<Cing, Cong>()
                .Compile<Cing, Cong>();
        }

        [Benchmark]
        public Task SendingRequests()
        {
            return _mediator.Send(_request);
        }

        [Benchmark]
        public Task PublishingNotifications()
        {
            return _mediator.Publish(_notification);
        }
        
        
        [Benchmark]
        public Task<Cong> SendingUncompiledRequests()
        {
            return _mediator.Send(_cequest);
        }
        
        [Benchmark]
        public Task<Cong> SendingCompiledRequests()
        {
            return _compiledPipeline.Compiled<Cing, Cong>()(_cequest);
        }
        
        [Benchmark]
        public async Task<Cong> SendingUncompiledRequestsAsync()
        {
            return await _mediator.Send(_cequest);
        }
        
        [Benchmark]
        public async Task<Cong> SendingCompiledRequestsAsync()
        {
            return await _compiledPipeline.Compiled<Cing, Cong>()(_cequest);
        }
    }
}
