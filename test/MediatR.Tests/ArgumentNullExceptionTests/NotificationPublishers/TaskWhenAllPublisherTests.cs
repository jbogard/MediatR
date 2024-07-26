using MediatR.NotificationPublishers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Tests.ArgumentNullExceptionTests.NotificationPublishers;

public class TaskWhenAllPublisherTests
{
    private sealed record Command : INotification;

    private readonly TaskWhenAllPublisher _taskWhenAllPublisher = new();

    [Fact]
    public async Task Should_throw_for_publish_when_handler_executors_is_null()
    {
        IEnumerable<NotificationHandlerExecutor> handlerExecutors = null!;
        INotification notification = new Command();

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await _taskWhenAllPublisher.Publish(handlerExecutors, notification, default(CancellationToken)));

        Assert.Equal(nameof(handlerExecutors), exception.ParamName);
    }

    [Fact]
    public async Task Should_throw_for_publish_when_notification_is_null()
    {
        IEnumerable<NotificationHandlerExecutor> handlerExecutors = new List<NotificationHandlerExecutor>();
        INotification notification = null!;

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            async () => await _taskWhenAllPublisher.Publish(handlerExecutors, notification, default(CancellationToken)));

        Assert.Equal(nameof(notification), exception.ParamName);
    }
}