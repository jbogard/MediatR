using MediatR.Extensions.Microsoft.DependencyInjection.Tests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Reflection.PortableExecutable;
using MediatR.Tests.MicrosoftExtensionsDI;

namespace MediatR.Tests
{
    public class GenericRequestHandlerTests : BaseGenericRequestHandlerTests
    {

       

        [Theory]
        [InlineData(9, 3, 3)]
        [InlineData(10, 4, 4)]
        [InlineData(1, 1, 1)]
        [InlineData(50, 3, 3)]
        public void ShouldRegisterTheCorrectAmountOfHandlers(int numberOfClasses, int numberOfInterfaces, int numberOfTypeParameters)
        {  
            var dynamicAssembly = GenerateCombinationsTestAssembly(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);          
            int expectedCombinations = CalculateTotalCombinations(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);
            var testClasses = Enumerable.Range(1, numberOfClasses)
               .Select(i => dynamicAssembly.GetType($"TestClass{i}")!)
               .ToArray();
            var combinations = GenerateCombinations(testClasses, numberOfInterfaces);
            combinations.Count.ShouldBe(expectedCombinations, $"Should have tested all {expectedCombinations} combinations");
        }

        [Theory]
        [InlineData(9, 3, 3)]
        [InlineData(10, 4, 4)]
        [InlineData(1, 1, 1)]
        [InlineData(50, 3, 3)]
        public void ShouldNotRegisterDuplicateHandlers(int numberOfClasses, int numberOfInterfaces, int numberOfTypeParameters)
        {
            var dynamicAssembly = GenerateCombinationsTestAssembly(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);
            int expectedCombinations = CalculateTotalCombinations(numberOfClasses, numberOfInterfaces, numberOfTypeParameters);
            var testClasses = Enumerable.Range(1, numberOfClasses)
               .Select(i => dynamicAssembly.GetType($"TestClass{i}")!)
               .ToArray();
            var combinations = GenerateCombinations(testClasses, numberOfInterfaces);
            var hasDuplicates = combinations
              .Select(x => string.Join(", ", x.Select(y => y.Name)))
              .GroupBy(x => x)
              .Any(g => g.Count() > 1);

            hasDuplicates.ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotRegisterGenericHandlersWhenOptingOut()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(new Logger());

            var assembly = GenerateOptOutAssembly();
            services.AddMediatR(cfg =>
            {
                //opt out flag set
                cfg.RegisterGenericHandlers = false;
                cfg.RegisterServicesFromAssembly(assembly);
            });

            var provider = services.BuildServiceProvider();
            var testClasses = Enumerable.Range(1, 2)
                .Select(i => assembly.GetType($"TestClass{i}")!)
                .ToArray();
            var requestType = assembly.GetType("OptOutRequest")!;
            var combinations = GenerateCombinations(testClasses, 2);

            var concreteRequestType = requestType.MakeGenericType(combinations.First());
            var requestHandlerInterface = typeof(IRequestHandler<>).MakeGenericType(concreteRequestType);

            var handler = provider.GetService(requestHandlerInterface);
            handler.ShouldBeNull($"Handler for {concreteRequestType} should be null");


        }
    }
}
