using System;

namespace MediatR.DependencyInjection.Configuration;

/// <summary>
/// Defines the Assembly Scanner options
/// </summary>
[Flags]
public enum AssemblyScannerOptions
{
    /// <summary>
    /// Scan for all services.
    /// </summary>
    All = PipelineBehaviors | Processor | Handlers | ExceptionActionHandler | ExceptionHandler,

    /// <summary>
    /// Scans for all request pipeline behaviors.
    /// </summary>
    RequestPipelineBehavior = 1,

    /// <summary>
    /// Scans for all request response pipeline behaviors.
    /// </summary>
    RequestResponsePipelineBehavior = 2,

    /// <summary>
    /// Scans for all stream request pipeline behaviors.
    /// </summary>
    StreamRequestPipelineBehavior = 4,

    /// <summary>
    /// Scans for all pipeline behaviors.
    /// </summary>
    PipelineBehaviors = RequestPipelineBehavior | RequestResponsePipelineBehavior | StreamRequestPipelineBehavior,

    /// <summary>
    /// Scans for all request pre processors.
    /// </summary>
    RequestPreProcessor = 8,

    /// <summary>
    /// Scans for all request post processors.
    /// </summary>
    RequestPostProcessor = 16,

    /// <summary>
    /// Scans for all request response pre processors.
    /// </summary>
    RequestResponsePreProcessor = 32,

    /// <summary>
    /// Scans for all request response post processor.
    /// </summary>
    RequestResponsePostProcessor = 64,

    /// <summary>
    /// Scans for all Processors.
    /// </summary>
    Processor = RequestPreProcessor | RequestPostProcessor | RequestResponsePreProcessor | RequestResponsePostProcessor,

    /// <summary>
    /// Scans for all notification handlers.
    /// </summary>
    NotificationHandler = 128,

    /// <summary>
    /// Scans for all request handlers.
    /// </summary>
    RequestHandler = 256,

    /// <summary>
    /// Scans for all request response handlers.
    /// </summary>
    RequestResponseHandler = 512,

    /// <summary>
    /// Scans for all stream request handlers.
    /// </summary>
    StreamRequestHandler = 1024,

    /// <summary>
    /// Scans for all Message handlers.
    /// </summary>
    Handlers = NotificationHandler | RequestHandler | RequestResponseHandler | StreamRequestHandler,

    /// <summary>
    /// Scans for all request exception actions handlers.
    /// </summary>
    RequestExceptionActionHandler = 2048,

    /// <summary>
    /// Scans for all request response exception actions handlers.
    /// </summary>
    RequestResponseExceptionActionHandler = 4096,

    /// <summary>
    /// Scans for all exception actions handlers.
    /// </summary>
    ExceptionActionHandler = RequestExceptionActionHandler | RequestResponseExceptionActionHandler,

    /// <summary>
    /// Scans for all request exception handlers.
    /// </summary>
    RequestExceptionHandler = 8192,

    /// <summary>
    /// Scans for all request response exception handlers.
    /// </summary>
    RequestResponseExceptionHandler = 16384,

    /// <summary>
    /// Scans for all exception handlers.
    /// </summary>
    ExceptionHandler = RequestExceptionHandler | RequestResponseExceptionHandler,
}