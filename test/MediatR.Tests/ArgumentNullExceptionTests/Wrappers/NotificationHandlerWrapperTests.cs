using MediatR.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.Wrappers;

public class NotificationHandlerWrapperTests
{
    private class Pinged : INotification
    {
    }

    [Fact]
    public async Task Should_throw_for_handle_when_notification_is_null()
    {
        INotification notification = null!;
        NotificationHandlerWrapperImpl<Pinged> notificationHandlerWrapperImpl = new();
        var serviceCollection = new ServiceCollection();
        var serviceFactory = serviceCollection.BuildServiceProvider();
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish = (_, _, _) => Task.CompletedTask;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await notificationHandlerWrapperImpl.Handle(notification, serviceFactory, publish, CancellationToken.None));

        Assert.Equal(nameof(notification), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_service_factory_is_null()
    {
        var notification = new Pinged();
        NotificationHandlerWrapperImpl<Pinged> notificationHandlerWrapperImpl = new();
        IServiceProvider serviceFactory = null!;
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish = (_, _, _) => Task.CompletedTask;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await notificationHandlerWrapperImpl.Handle(notification, serviceFactory, publish, CancellationToken.None));

        Assert.Equal(nameof(serviceFactory), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_handle_when_publish_is_null()
    {
        var notification = new Pinged();
        NotificationHandlerWrapperImpl<Pinged> notificationHandlerWrapperImpl = new();
        var serviceCollection = new ServiceCollection();
        var serviceFactory = serviceCollection.BuildServiceProvider();
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await notificationHandlerWrapperImpl.Handle(notification, serviceFactory, publish, CancellationToken.None));

        Assert.Equal(nameof(publish), exception.ParamName);
    }
}