using Grpc.Net.Client;

namespace MediatR.Router.GRPC;

public class RemoteServiceDefinition
{
  public string Uri { get; set; }
  public GrpcChannelOptions ChannelOptions { get; set; }
}