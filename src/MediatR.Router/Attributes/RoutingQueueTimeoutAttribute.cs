using System;

namespace MediatR.Router
{
  [AttributeUsage(AttributeTargets.Class)]
  public class RoutingQueueTimeoutAttribute : System.Attribute
  {
    public int ConsumerTimeout { get; set; }
  }
}