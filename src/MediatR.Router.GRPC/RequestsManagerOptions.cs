using System;
using System.Collections.Generic;

namespace MediatR.Router.GRPC;

public class RequestsManagerOptions
{
  public HashSet<Type> AcceptMessageTypes { get; private set; } = new HashSet<Type>();
}