using System;

namespace MediatR.NotificationHandlersOrder
{
    public class NotificationOrderAttribute : Attribute
    {
        public int Value { get; }

        public NotificationOrderAttribute(int value)
        {
            Value = value;
        }
    }
}