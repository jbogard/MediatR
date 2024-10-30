using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Grpc.AspNetCore.Server;
using MediatR;
using MediatR.Router;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatR.Router.GRPC.Extensions
{
  public static class Extensions
  {
    public const string MediatRGrpcCorsDefaultPolicy = "MediatRGRPCDefault";

    public static void AddMediatorGrpcCors(this CorsOptions corsOptions)
    {
      corsOptions.AddPolicy(MediatRGrpcCorsDefaultPolicy, builder =>
      {
        builder.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
          .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
      });
    }

    public static MessageDispatcherOptions DispatchOnly(this MessageDispatcherOptions options,
      Func<IEnumerable<Assembly>> assemblySelect)
    {
      var types = (
        from a in assemblySelect()
        from t in a.GetTypes()
        where typeof(IBaseRequest).IsAssignableFrom(t)
        select t).AsEnumerable();

      foreach (var t in types)
        options.DispatchOnly.Add(t);

      return options;
    }

    public static MessageDispatcherOptions DispatchOnly(this MessageDispatcherOptions options,
      Func<IEnumerable<Type>> typesSelect)
    {
      foreach (var type in typesSelect().Where(t => typeof(IBaseRequest).IsAssignableFrom(t)))
        options.DispatchOnly.Add(type);

      return options;
    }

    public static MessageDispatcherOptions DenyDispatch(this MessageDispatcherOptions options,
      Func<IEnumerable<Type>> typesSelect)
    {
      foreach (var type in typesSelect().Where(t => typeof(IBaseRequest).IsAssignableFrom(t)))
        options.DispatchOnly.Add(type);

      return options;
    }

    public static MessageDispatcherOptions DenyDispatch(this MessageDispatcherOptions options,
      Func<IEnumerable<Assembly>> assemblySelect)
    {
      var types = (
        from a in assemblySelect()
        from t in a.GetTypes()
        where typeof(IBaseRequest).IsAssignableFrom(t)
        select t).AsEnumerable();

      foreach (var t in types)
        options.DontDispatch.Add(t);

      return options;
    }


    public static RequestsManagerOptions AcceptedMessages(this RequestsManagerOptions options,
      Func<IEnumerable<Assembly>> assemblySelect)
    {
      var types = (
        from a in assemblySelect()
        from t in a.GetTypes()
        where typeof(IBaseRequest).IsAssignableFrom(t)
        select t).AsEnumerable();

      foreach (var t in types)
        options.AcceptMessageTypes.Add(t);

      return options;
    }

    public static RequestsManagerOptions AcceptedMessages(this RequestsManagerOptions options,
      Func<IEnumerable<Type>> typesSelect)
    {
      foreach (var type in typesSelect().Where(t => typeof(IBaseRequest).IsAssignableFrom(t)))
        options.AcceptMessageTypes.Add(type);

      return options;
    }

    /// <summary>
    /// Add the Arbitrer RabbitMQ message dispatcher to the service collection, allowing it to be resolved and used.
    /// </summary>
    /// <param name="services">The service collection to add the message dispatcher to.</param>
    /// <param name="config">The configuration settings for the message dispatcher.</param>
    /// <param name="grpcOptions">Grpc service configuration options, if not specified you need to call AddGrpc()
    /// registration method.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMediatorGrpcDispatcher(this IServiceCollection services,
      Action<MessageDispatcherOptions> config,
      Action<GrpcServiceOptions<MessageDispatcher>> grpcOptions = null)
    {
      var gbuilder = services.AddGrpc();
      if (grpcOptions != null)
      {
        gbuilder.AddServiceOptions<MessageDispatcher>(grpcOptions);
      }

      services.AddCors(o => o.AddMediatorGrpcCors());

      services.Configure<MessageDispatcherOptions>(config);
      services.AddKeyedSingleton<IExternalMessageDispatcher, MessageDispatcher>(OOPRouter.OopMessageDispatchersServiceKey);

      return services;
    }

    public static IServiceCollection AddGrpcRequestManager(this IServiceCollection services, Action<RequestsManagerOptions> options,
      Action<GrpcServiceOptions<RequestsManager>> grpcOptions = null)
    {
      if (options != null)
        services.Configure(options);

      var gbuilder = services.AddGrpc();
      if (grpcOptions != null)
      {
        gbuilder.AddServiceOptions<RequestsManager>(grpcOptions);
      }

      return services;
    }

    public static IEndpointRouteBuilder UseGrpcRequestManager(this IEndpointRouteBuilder host)
    {
      host.MapGrpcService<RequestsManager>();
      return host;
    }

    public static IEndpointRouteBuilder UseGrpcWebRequestsManager(this IEndpointRouteBuilder host)
    {
      host.MapGrpcService<RequestsManager>()
        .EnableGrpcWeb()
        .RequireCors(MediatRGrpcCorsDefaultPolicy);
      return host;
    }

    public static string GetHash(this string input, HashAlgorithm hashAlgorithm)
    {
      byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
      var sBuilder = new StringBuilder();

      for (int i = 0; i < data.Length; i++)
      {
        sBuilder.Append(data[i].ToString("x2"));
      }

      return sBuilder.ToString();
    }
  }
}