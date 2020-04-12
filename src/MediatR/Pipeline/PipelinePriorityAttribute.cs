using System;

namespace MediatR.Pipeline
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Interface), AllowMultiple = false, Inherited = true)]
    public class PipelinePriorityAttribute : Attribute
    {
        public PipelinePriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; set; }
    }

    public static class PipelinePriorityOrder
    {
        public const int Normal = 0;
        public const int High = 100;
        public const int Low = -100;
        internal const int RequestPreProcessor = int.MaxValue;
        internal const int RequestPostProcessor = int.MaxValue - 1;
        internal const int RequestExceptionActionProcessor = int.MaxValue - 2;
        internal const int RequestExceptionProcessor = int.MaxValue - 3;
    }
}
