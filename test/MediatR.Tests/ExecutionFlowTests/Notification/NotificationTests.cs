using FluentAssertions;
using MediatR.Abstraction;
using MediatR.DependencyInjection;
using MediatR.MicrosoftDICExtensions;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MediatR.Tests.ExecutionFlowTests;

[TestFixture]
internal sealed class NotificationTests
{
    [Test]
    public void PublishNotification_WithDefaultPublisher_ExecutesAllNotificationTypeHandlers()
    {
        const string NotExplicitHandledMessage = "Notification was not handled.";
        // Arrange
        var collection = new ServiceCollection();
        var provider = ConfigureMediator(collection).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new Notification
        {
            Message = NotExplicitHandledMessage
        };
        
        // Act
        mediator.Publish(notification);
        
        // Assert
        var baseHandler = provider.GetRequiredService<BaseNotificationHandler>();
        var multiHandler = provider.GetRequiredService<MultiNotificationHandler>();

        notification.Message.Should().NotBe(NotExplicitHandledMessage);
        notification.Handlers.Should().Be(2);
        baseHandler.Calls.Should().Be(0); // Must be never called because handled notification type is not the same as the published one.
        multiHandler.Calls.Should().Be(1); // Handles only the final notification type.
        notification.GenericHandlerType.Should().Be(typeof(GenericNotificationHandler<Notification>));
    }

    [Test]
    public void PublishNotificationAsOtherType_WIthDefaultPublisher_ExecutesAllOtherTypedNotificationHandlers()
    {
        const string NotExplicitHandledMessage = "Notification was not handled.";
        // Arrange
        var collection = new ServiceCollection();
        var provider = ConfigureMediator(collection).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new Notification
        {
            Message = NotExplicitHandledMessage
        };
        
        // Act
        mediator.Publish<BaseNotification>(notification);
        
        // Assert
        var baseHandler = provider.GetRequiredService<BaseNotificationHandler>();
        var multiHandler = provider.GetRequiredService<MultiNotificationHandler>();

        notification.Message.Should().Be(NotExplicitHandledMessage);
        notification.Handlers.Should().Be(3);
        baseHandler.Calls.Should().Be(1); // Must be called because is handled notification base type.
        multiHandler.Calls.Should().Be(1); // Handles only the base notification type.
        GenericNotificationHandler<BaseNotification>.Calls.Should().Be(1); // Must be called because is handled notification base type.
    }
    
    [Test]
    public void PublishNotification_WIthTaskWhenAllPublisher_ExecutedAllHandlesAtOnce()
    {
        // Arrange
        var collection = new ServiceCollection();
        var expectedNotificationPublisher = new TaskWhenAllPublisher();
        var provider = ConfigureMediator(collection, expectedNotificationPublisher).BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new Notification
        {
            Message = "some Message"
        };
        
        // Act
        mediator.Publish(notification);
        
        // Assert
        var notificationPublisher = provider.GetRequiredService<INotificationPublisher>();
        notificationPublisher.Should().Be(expectedNotificationPublisher);
        notification.GenericHandlerType.Should().Be(typeof(GenericNotificationHandler<Notification>));
        notification.Handlers.Should().Be(2);
    }

    private static IServiceCollection ConfigureMediator(IServiceCollection serviceCollection, INotificationPublisher? notificationPublisher = null) =>
        serviceCollection.ConfigureMediatR(c =>
        {
            c.RegisterServicesFromAssemblyContaining<NotificationTests>();
            c.RegistrationOptions = RegistrationOptions.SingletonAndMapped;
            if (notificationPublisher is not null)
            {
                c.NotificationPublisher = notificationPublisher;
            }
        });
}