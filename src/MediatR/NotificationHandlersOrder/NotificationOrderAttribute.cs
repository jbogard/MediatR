namespace MediatR.NotificationHandlersOrder
{
    using System;

    /// <summary>
    /// Defines a value of the processing order of notification handlers
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NotificationHandlerOrderAttribute : Attribute
    {
        public int Value { get; }

        public NotificationHandlerOrderAttribute(int value)
        {
            Value = value;
        }
    }
}