

using System;

namespace MediatR.Router.Messages
{
  /// <summary>
  /// The ResponseMessage class represents a response message that is returned from various operations.
  /// It contains information about the status, content, and any associated exception. </summary> <typeparam name="T">The type of the content</typeparam>
  /// /
  public class ResponseMessage<T>
  {
    public StatusEnum Status { get; set; }
    public T Content { get; set; }
    public Exception Exception { get; set; }
  }


  /// <summary>
  /// Represents a response message from a service.
  /// </summary>
  public class ResponseMessage
  {
    public StatusEnum Status { get; set; }
    public object Content { get; set; }
    public Exception Exception { get; set; }
  }

  /// <summary>
  /// Represents the status values for a certain operation.
  /// </summary>
  public enum StatusEnum
  {
    Ok,
    Exception
  }
}