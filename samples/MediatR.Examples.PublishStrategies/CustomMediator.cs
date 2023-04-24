using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.PublishStrategies;

public class CustomMediator : Mediator
{
    private readonly Func<IEnumerable<NotificationHandlerExecutor>, object, CancellationToken, Task> _publish;

    public CustomMediator(IServiceProvider serviceFactory, Func<IEnumerable<NotificationHandlerExecutor>, object, CancellationToken, Task> publish) : base(serviceFactory)
        => _publish = publish;

    protected override Task PublishCore(IEnumerable<NotificationHandlerExecutor> handlerExecutors, object notification, CancellationToken cancellationToken)
        => _publish(handlerExecutors, notification, cancellationToken);
}