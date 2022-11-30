namespace MediatR.Examples.PublishStrategies;

/// <summary>
/// Strategy to use when publishing notifications
/// </summary>
public enum PublishStrategy
{
    /// <summary>
    /// Run each notification handler after one another. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
    /// </summary>
    SyncContinueOnException = 0,

    /// <summary>
    /// Run each notification handler after one another. Returns when all handlers are finished or an exception has been thrown. In case of an exception, any handlers after that will not be run.
    /// </summary>
    SyncStopOnException = 1,

    /// <summary>
    /// Run all notification handlers asynchronously. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
    /// </summary>
    Async = 2,

    /// <summary>
    /// Run each notification handler on its own thread using Task.Run(). Returns immediately and does not wait for any handlers to finish. Note that you cannot capture any exceptions, even if you await the call to Publish.
    /// </summary>
    ParallelNoWait = 3,

    /// <summary>
    /// Run each notification handler on its own thread using Task.Run(). Returns when all threads (handlers) are finished. In case of any exception(s), they are captured in an AggregateException by Task.WhenAll.
    /// </summary>
    ParallelWhenAll = 4,

    /// <summary>
    /// Run each notification handler on its own thread using Task.Run(). Returns when any thread (handler) is finished. Note that you cannot capture any exceptions (See msdn documentation of Task.WhenAny)
    /// </summary>
    ParallelWhenAny = 5,
}
