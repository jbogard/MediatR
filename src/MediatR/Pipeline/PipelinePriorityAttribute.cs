using System;

namespace MediatR.Pipeline
{
    /// <summary>
    /// Controls the order of execution for <see cref="IPipelineBehavior{TRequest,TResponse}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PipelinePriorityAttribute : Attribute
    {
        public PipelinePriorityAttribute(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// Priority of execution. Higher the the number the higher the priority. See <see cref="PipelinePriorityOrder"/> for predefined priorities.
        /// </summary>
        public int Priority { get; set; }
    }

    public static class PipelinePriorityOrder
    {
        public const int Normal = 0;
        public const int High = 100;
        public const int Low = -100;
        public const int RequestPreProcessor = int.MaxValue;
        public const int RequestPostProcessor = int.MaxValue - 1;
        public const int RequestExceptionActionProcessor = int.MaxValue - 2;
        public const int RequestExceptionProcessor = int.MaxValue - 3;
    }
}
