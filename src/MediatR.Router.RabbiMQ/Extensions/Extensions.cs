using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Router.RabbitMQ
{
  /// <summary>
  /// Provides extension methods for various classes.
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Add the Arbitrer RabbitMQ message dispatcher to the service collection, allowing it to be resolved and used.
    /// </summary>
    /// <param name="services">The service collection to add the message dispatcher to.</param>
    /// <param name="config">The configuration settings for the message dispatcher.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddArbitrerRabbitMQMessageDispatcher(this IServiceCollection services,
      Action<MessageDispatcherOptions> config)
    {
      services.Configure<MessageDispatcherOptions>(config);
      services.AddKeyedSingleton<IExternalMessageDispatcher, MessageDispatcher>(OOPRouter.OopMessageDispatchersServiceKey);
      return services;
    }

    public static MessageDispatcherOptions DispatchOnlyTo(this MessageDispatcherOptions options,
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

    public static MessageDispatcherOptions DispatchOnlyTo(this MessageDispatcherOptions options,
      Func<IEnumerable<Type>> typesSelect)
    {
      foreach (var type in typesSelect().Where(t => typeof(IBaseRequest).IsAssignableFrom(t)))
        options.DispatchOnly.Add(type);

      return options;
    }

    public static MessageDispatcherOptions DenyDispatchTo(this MessageDispatcherOptions options,
      Func<IEnumerable<Type>> typesSelect)
    {
      foreach (var type in typesSelect().Where(t => typeof(IBaseRequest).IsAssignableFrom(t)))
        options.DispatchOnly.Add(type);

      return options;
    }

    public static MessageDispatcherOptions DenyDispatchTo(this MessageDispatcherOptions options,
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

    /// <summary>
    /// Resolves the arbitrer calls by adding the RequestManager as a hosted service to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the RequestManager will be added.</param>
    /// <returns>The modified service collection.</returns>
    [Obsolete("use AddRabbitMQRequestManager method instead")]
    public static IServiceCollection ResolveArbitrerCalls(this IServiceCollection services)
    {
      services.AddHostedService<RequestsManager>();
      return services;
    }

    public static IServiceCollection AddRabbitMQRequestManager(this IServiceCollection services)
    {
      services.AddHostedService<RequestsManager>();
      return services;
    }
    
    /// <summary>
    /// Computes the hash value of a string using the specified HashAlgorithm.
    /// </summary>
    /// <param name="input">The input string to be hashed.</param>
    /// <param name="hashAlgorithm">The HashAlgorithm to be used for computing the hash value.</param>
    /// <returns>
    /// The hexadecimal representation of the computed hash value.
    /// </returns>
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

    /// <summary>
    /// Computes the hash value for the specified byte array using the specified hashing algorithm.
    /// </summary>
    /// <param name="input">The input byte array to compute the hash for.</param>
    /// <param name="hashAlgorithm">The hashing algorithm to use.</param>
    /// <returns>
    /// A string representation of the computed hash value.
    /// </returns>
    public static string GetHash(this byte[] input, HashAlgorithm hashAlgorithm)
    {
      byte[] data = hashAlgorithm.ComputeHash(input);
      var sBuilder = new StringBuilder();
      for (int i = 0; i < data.Length; i++)
      {
        sBuilder.Append(data[i].ToString("x2"));
      }

      return sBuilder.ToString();
    }
  }
}