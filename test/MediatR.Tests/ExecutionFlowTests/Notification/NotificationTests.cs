using System;
using System.Linq;
using FluentAssertions;
using MediatR.Abstraction;
using MediatR.Abstraction.Handlers;
using MediatR.DependencyInjection.ConfigurationBase;
using MediatR.MicrosoftDiCExtensions;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.ExecutionFlowTests.Notification;

public sealed class NotificationTests
{
    [Fact]
    public void PublishNotification_WithDefaultPublisher_ExecutesAllNotificationTypeHandlers()
    {
        const string NotExplicitHandledMessage = "Notification was not handled.";
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.RegisterServicesFromAssemblyContaining<NotificationTests>();
        });
        var provider = collection.BuildServiceProvider();

        var notification = new Notification
        {
            Message = NotExplicitHandledMessage
        };

        // Act
        provider.GetRequiredService<IMediator>().Publish(notification);

        // Assert
        var baseHandler = provider.GetRequiredService<BaseNotificationHandler>();
        var multiHandler = provider.GetRequiredService<MultiNotificationHandler>();
        var genericHandlerType = GetGenericNotificationHandler<Notification>(provider);

        notification.Message.Should().NotBe(NotExplicitHandledMessage);
        notification.Handlers.Should().Be(2);

        baseHandler.Calls.Should().Be(0);
        multiHandler.Calls.Should().Be(1);
        genericHandlerType.Calls.Should().Be(1);
    }

    [Fact]
    public void PublishNotification_WithCachedHandler_CreatesOnlyOnceHandlerInstance()
    {
        const string NotExplicitHandledMessage = "Notification was not handled.";
        // Arrange
        var collection = new ServiceCollection();
        collection.AddMediatR(cfg =>
        {
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.EnableCachingOfHandlers = true;
            cfg.RegisterServicesFromAssemblyContaining<NotificationTests>();
        });
        var provider = collection.BuildServiceProvider();

        var notification = new Notification
        {
            Message = NotExplicitHandledMessage
        };
        
        var sut = provider.GetRequiredService<IMediator>();

        // Act
        sut.Publish(notification);
        sut.Publish(notification);

        // Assert
        var baseHandler = provider.GetRequiredService<BaseNotificationHandler>();
        var multiHandler = provider.GetRequiredService<MultiNotificationHandler>();
        var genericHandler = GetGenericNotificationHandler<Notification>(provider);

        notification.Message.Should().NotBe(NotExplicitHandledMessage);
        notification.Handlers.Should().Be(4);
        genericHandler.Calls.Should().Be(2);
        baseHandler.Calls.Should().Be(0);
        multiHandler.Calls.Should().Be(2);
    }

    [Fact]
    public void PublishNotification_WithTaskWhenAllPublisher_ExecutedAllHandlesAtOnce()
    {
        // Arrange
        var collection = new ServiceCollection();
        var expectedNotificationPublisher = new TaskWhenAllPublisher();
        var provider = collection.AddMediatR(cfg =>
        {
            cfg.RegistrationStyle = RegistrationStyle.OneInstanceForeachService;
            cfg.NotificationPublisher = expectedNotificationPublisher;
            cfg.RegisterServicesFromAssemblyContaining<NotificationTests>();
        }).BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new Notification
        {
            Message = "some Message"
        };

        // Act
        mediator.Publish(notification);

        // Assert
        var notificationPublisher = provider.GetRequiredService<INotificationPublisher>();
        var genericNotificationHandler = GetGenericNotificationHandler<Notification>(provider);
        var multiHandler = provider.GetRequiredService<MultiNotificationHandler>();

        notificationPublisher.Should().Be(expectedNotificationPublisher);
        notification.Handlers.Should().Be(2);

        genericNotificationHandler.Calls.Should().Be(1);
        multiHandler.Calls.Should().Be(1);
    }

    private GenericNotificationHandler<TNotification> GetGenericNotificationHandler<TNotification>(IServiceProvider serviceProvider)
        where TNotification : INotification =>
        (GenericNotificationHandler<TNotification>) serviceProvider
            .GetServices<INotificationHandler<TNotification>>()
            .Single(t => t is GenericNotificationHandler<TNotification>);
}