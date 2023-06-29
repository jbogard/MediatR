using System;

namespace MediatR.Tests.ExecutionFlowTests;

internal abstract class BaseNotification : INotification
{
    public int Handlers { get; set; }
    public Type? GenericHandlerType { get; set; }
}