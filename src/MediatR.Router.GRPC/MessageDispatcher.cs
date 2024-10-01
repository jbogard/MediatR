using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Grpc.Core;
using Grpc.Net.Client;
using MediatR.Router;
using MediatR.Router.GRPC;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MediatR.Router.GRPC
{
  /// <summary>
  /// Class for dispatching messages to RabbitMQ and handling responses.
  /// </summary>
  public class MessageDispatcher : IExternalMessageDispatcher, IDisposable
  {
    private readonly MessageDispatcherOptions options;
    private readonly ILogger<MessageDispatcher> logger;

    private readonly RouterOptions routerOptions;

    public MessageDispatcher(
      IOptions<MessageDispatcherOptions> options,
      ILogger<MessageDispatcher> logger, IOptions<RouterOptions> routerOptions)
    {
      this.options = options.Value;
      this.logger = logger;
      this.routerOptions = routerOptions.Value;

      DestinationChannels.Add(this.options.DefaultServiceUri,
        GrpcChannel.ForAddress(this.options.DefaultServiceUri, this.options.ChannelOptions));
    }


    public Dictionary<string, GrpcChannel> DestinationChannels { get; set; } = new();

    public GrpcChannel GetChannelFor<T>()
    {
      if (this.options.RemoteTypeServices.TryGetValue(typeof(T), out var service))
      {
        if (!DestinationChannels.ContainsKey(service.Uri))
          DestinationChannels.Add(service.Uri, GrpcChannel.ForAddress(service.Uri, service.ChannelOptions));

        return DestinationChannels[service.Uri];
      }

      return DestinationChannels[options.DefaultServiceUri];
    }

    public Dictionary<string, GrpcServices.GrpcServicesClient> DestinationClients { get; set; } = new();

    public GrpcServices.GrpcServicesClient GetClientFor<T>()
    {
      var serviceuri = options.DefaultServiceUri;
      if (this.options.RemoteTypeServices.TryGetValue(typeof(T), out var service))
      {
        serviceuri = service.Uri;
      }

      if (!DestinationClients.ContainsKey(serviceuri))
        DestinationClients.Add(serviceuri, new GrpcServices.GrpcServicesClient(GetChannelFor<T>()));
      return DestinationClients[serviceuri];
    }


    public bool CanDispatch<TRequest>()
    {
      if (options.DispatchOnly.Count > 0)
        return options.DispatchOnly.Contains(typeof(TRequest));

      if (options.DontDispatch.Count > 0)
        return !options.DontDispatch.Contains(typeof(TRequest));

      return true;
    }

    public async Task<Messages.ResponseMessage<TResponse>> Dispatch<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
    {
      var message = JsonConvert.SerializeObject(request, options.SerializerSettings);

      var grpcClient = GetClientFor<TRequest>();
      var result = await grpcClient.ManageRoutedMessageAsync(new RequestMessage
      {
        Body = message,
        RoutingType = typeof(TRequest).TypeQueueName(routerOptions)
      });
      return JsonConvert.DeserializeObject<Messages.ResponseMessage<TResponse>>(result.Body, options.SerializerSettings);
    }

    /// <summary>
    /// Sends a notification message to the specified exchange and routing key.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message.</typeparam>
    /// <param name="request">The request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the notification operation.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    public Task Notify<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : INotification
    {
      var message = JsonConvert.SerializeObject(request, options.SerializerSettings);

      logger.LogInformation($"Sending notifications of: {typeof(TRequest).Name}/{request.GetType().TypeQueueName(routerOptions)}");

      foreach (var channel in DestinationChannels)
      {
        var grpcClient = GetClientFor<TRequest>();
        grpcClient.ManageRoutedNotificationAsync(new NotifyMessage()
        {
          Body = message,
          RoutingType = typeof(TRequest).TypeQueueName(routerOptions)
        });
      }

      return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
  }
}