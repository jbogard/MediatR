using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MediatR.Abstraction;
using MediatR.Benchmarks.MockServices.Request;
using MediatR.Benchmarks.MockServices.RequestResponse;
using MediatR.DependencyInjection.Configuration;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Benchmarks.Mediator;

[MemoryDiagnoser]
public class MediatR_ExceptionHanding_Benchmarks
{
    private readonly ThrowingPing _request = new();
    private readonly ThrowingPingPong _requestResponse = new();

    private readonly IMediator _mediator;

    [Params(1, 50)]
    public int Times { get; set; }

    public MediatR_ExceptionHanding_Benchmarks()
    {
        var collection = new ServiceCollection();

        collection.AddMediatR(conf =>
        {
            conf.RegisterServicesFromAssemblyContaining<MediatR_Handlers_Benchmarks>(AssemblyScannerOptions.Handlers | AssemblyScannerOptions.ExceptionHandler);
            conf.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            conf.DefaultServiceLifetime = ServiceLifetime.Singleton;
            conf.EnableCachingOfHandlers = true;
        });

        var provider = collection.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task SendingRequests()
    {
        for (var i = 0; i < Times; i++)
        {
            await _mediator.SendAsync(_request);
        }
    }

    [Benchmark]
    public async Task SendingRequestResponse()
    {
        for (var i = 0; i < Times; i++)
        {
            _ = await _mediator.SendAsync(_requestResponse);
        }
    }
}