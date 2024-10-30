using System;
using System.Collections.Generic;
using Grpc.Net.Client;
using Newtonsoft.Json;

namespace MediatR.Router.GRPC
{
  public class MessageDispatcherOptions
  {
    /// <summary>
    /// Gets or sets the time-to-live value for deduplication.
    /// </summary>
    /// <remarks>
    /// The DeDuplicationTTL property determines the amount of time, in milliseconds, that an item can be considered duplicate
    /// before it is removed from the deduplication cache. The default value is 5000 milliseconds (5 seconds).
    /// </remarks>
    /// <value>
    /// The time-to-live value for deduplication.
    /// </value>
    public int DeDuplicationTTL { get; set; } = 5000;

    /// <summary>
    /// Gets or sets a value indicating whether duplicate entries are enabled for deduplication.
    /// </summary>
    /// <value>
    /// <c>true</c> if deduplication is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool DeDuplicationEnabled { get; set; } = true;

    public string DefaultServiceUri { get; set; }

    public GrpcChannelOptions ChannelOptions { get; set; } = new();

    public Dictionary<Type, RemoteServiceDefinition> RemoteTypeServices { get; set; } = new();

    public HashSet<Type> DispatchOnly { get; private set; } = new HashSet<Type>();
    public HashSet<Type> DontDispatch { get; private set; } = new HashSet<Type>();

    public JsonSerializerSettings SerializerSettings { get; set; }

    public MessageDispatcherOptions()
    {
      SerializerSettings = new JsonSerializerSettings()
      {
        MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
        DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc
      };
      SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    }
  }
}