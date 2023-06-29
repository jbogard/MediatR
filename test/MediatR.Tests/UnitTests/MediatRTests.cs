using FluentAssertions;
using MediatR.Abstraction;
using MediatR.MicrosoftDICExtensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MediatR.Tests.UnitTests;

[TestFixture]
internal sealed class MediatRTests
{
    [Test]
    public void DefaultMediatR_PublishedNullNotification_ThrowsNullReferenceException()
    {
        // Arrange
        var mediator = GetMediator();
        
        // Act
        var act = () => mediator.Publish((INotification)null!);
        
        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*notification*");
    }

    private static IMediator GetMediator()
    {
        var serviceColl = new ServiceCollection();
        serviceColl.ConfigureMediatR(c => c.RegisterServicesFromAssemblyContaining<MediatRTests>());
        return ServiceProviderServiceExtensions.GetRequiredService<IMediator>(serviceColl.BuildServiceProvider());
    }
}