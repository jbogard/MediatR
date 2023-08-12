using System;
using FluentAssertions;
using MediatR.Abstraction;
using MediatR.ExecutionFlowTests.Notification;
using MediatR.ExecutionFlowTests.RequestResponse;
using MediatR.ExecutionFlowTests.Requests.RequestMessages;
using MediatR.ExecutionFlowTests.StreamRequest.StreamRequestMessages;
using MediatR.MicrosoftDependencyInjectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.UnitTests;

public sealed class MediatRTests
{
    private readonly IMediator _sut;

    public MediatRTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatRTests>());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<IMediator>();
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
    public void DefaultMediatR_PublishNullObjectNotification_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        var act = () => _sut.Publish(null!);

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
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*request*");
    }

    [Fact]
    public void DefaultMediatR_PublishedNullObjectRequestResponse_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        var act = async () => await _sut.SendAsync(null!);
        
        // Assert
        act.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("*request*");
    }

    [Fact]
    public void DefaultMediatR_PublishedNullRequest_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        var act = () => _sut.SendAsync((Request)null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*request*");
    }

    [Fact]
    public void DefaultMediatR_PublishedNullStreamRequest_ThrowsArgumentNullException()
    {
        // Arrange
        
        // Act
        var act = () => _sut.CreateStreamAsync<StreamResponse>(null!);
        
        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*request*");
    }

    [Fact]
    public void DefaultMediatR_PublishedNullObjectStreamRequest_ThrowsArgumentNullException()
    {
        // Arrange
        
        // Act
        var act = () => _sut.CreateStreamAsync(null!);
        
        // Assert
        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*request*");
    }
}