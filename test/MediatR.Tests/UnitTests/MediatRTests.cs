using System;
using FluentAssertions;
using MediatR.Abstraction;
using MediatR.ExecutionFlowTests.Notification;
using MediatR.ExecutionFlowTests.RequestResponse;
using MediatR.MicrosoftDiCExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.UnitTests;

public sealed class MediatRTests
{
    private readonly IMediator _sut;
    private readonly ServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;

    public MediatRTests()
    {
        _serviceCollection = new ServiceCollection();
        _serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatRTests>());
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        _sut = _serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public void DefaultMediatR_PublishedNullNotification_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        var act = () => _sut.Publish<BaseNotification>(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*notification*");
    }

    [Fact]
    public void DefaultMediatR_PublishedNullRequestResponse_ThrowsArgumentNullException()
    {
        // Arrange
        
        // Act
        var act = () => _sut.SendAsync<Response>(null!);
        
        // Assert
        act.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("*request*");
    }
}