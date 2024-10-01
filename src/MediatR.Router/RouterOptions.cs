using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;

namespace MediatR.Router
{
  public class RouterOptions
  {
    public string DefaultQueuePrefix { get; set; } = String.Empty;
    
    /// <summary>
    /// Gets or sets the behaviour of the Router.
    /// </summary>
    /// <value>
    /// The behaviour of the Router.
    /// </value>
    public RouterBehaviourEnum Behaviour { get; set; } = RouterBehaviourEnum.ImplicitLocal;

    /// <summary>
    /// Gets or sets the collection of local requests.
    /// </summary>
    /// <value>The local requests.</value>
    public HashSet<Type> LocalRequests { get; private set; } = new HashSet<Type>();

    /// <summary>
    /// Gets the set of remote requests supported by the application.
    /// </summary>
    public HashSet<Type> RemoteRequests { get; private set; } = new HashSet<Type>();

    /// <summary>
    /// Get the prefix of remote queue
    /// </summary>
    public Dictionary<string, string> QueuePrefixes { get; private set; } = new Dictionary<string, string>();
  }


  /// <summary>
  /// Specifies the possible behaviours of an arbitrator.
  /// </summary>
  public enum RouterBehaviourEnum
  {
    ImplicitLocal,
    ImplicitRemote,
    Explicit
  }
}