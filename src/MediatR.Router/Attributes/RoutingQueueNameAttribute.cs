using System;

namespace MediatR.Router
{
  /// <summary>
  /// Represents an attribute used to specify the name of the Router queue for a class.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class RoutingQueueNameAttribute : System.Attribute
  {
    public string Name { get; set; }
  }
}