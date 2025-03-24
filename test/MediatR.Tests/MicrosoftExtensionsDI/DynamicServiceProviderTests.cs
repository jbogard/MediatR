using MediatR.Registration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static MediatR.Tests.SendTests;

namespace MediatR.Tests.MicrosoftExtensionsDI
{
    public class DynamicServiceProviderTests
    {
        private readonly DynamicServiceProvider _dynamicServiceProvider;

        public DynamicServiceProviderTests()
        {
            var services = new ServiceCollection();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(Pong).Assembly);
                cfg.RegisterGenericHandlers = true;
            });
            services.AddSingleton(new Dependency());
            var rootProvider = services.BuildServiceProvider();
            _dynamicServiceProvider = new DynamicServiceProvider(rootProvider);
        }

        [Fact]
        public void ShouldNotRescanForGenericHandlerAfterFirstRegistration()
        {
            var firstHandler = _dynamicServiceProvider.GetService<IRequestHandler<GenericPing<Pong>, Pong>>();
            firstHandler.ShouldNotBeNull();

            var secondHandler = _dynamicServiceProvider.GetService<IRequestHandler<GenericPing<Pong>, Pong>>();
           
            secondHandler!.GetType().ShouldBeSameAs(firstHandler.GetType());

            var serviceDescriptorCount = _dynamicServiceProvider
                .GetAllServiceDescriptors()
                .Count(sd => sd.ServiceType == typeof(IRequestHandler<GenericPing<Pong>, Pong>));

            serviceDescriptorCount.ShouldBe(1);
        }
    }
}
