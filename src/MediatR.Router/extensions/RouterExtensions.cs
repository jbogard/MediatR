using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable AssignNullToNotNullAttribute

namespace MediatR.Router
{
    /// <summary>
    /// Extension methods for configuring and using Router in an ASP.NET Core application.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static class RouterExtensions
    {
        /// <summary>
        /// Adds the Router to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional configuration action.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddMediatorRouter(this IServiceCollection services, Action<RouterOptions> configure = null)
        {
            if (configure != null)
                services.Configure<RouterOptions>(configure);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Pipelines.RouterPipeline<,>));
            services.AddSingleton<IRouter, OOPRouter>();

            services.AddTransient<IMediator, RoutedMediatr>();
            return services;
        }

        /// <summary>
        /// Adds the Router service to the specified <see cref="ServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="ServiceCollection"/> to add the Router service to.</param>
        /// <param name="assemblies">The collection of assemblies to use for inference.</param>
        /// <returns>The updated <see cref="ServiceCollection"/>.</returns>
        public static IServiceCollection AddMediatorRouter(this ServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddMediatorRouter(cfg =>
            {
                cfg.Behaviour = RouterBehaviourEnum.ImplicitRemote;
                cfg.InferLocalRequests(assemblies);
                cfg.InferLocalNotifications(assemblies);
            });
            return services;
        }

        /// <summary>
        /// Infers the local requests based on the provided assemblies and updates the options accordingly.
        /// </summary>
        /// <param name="options">The existing Router options.</param>
        /// <param name="assemblies">The assemblies to search for request handlers.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The updated Router options with the inferred local requests.</returns>
        public static RouterOptions InferLocalRequests(this RouterOptions options, IEnumerable<Assembly> assemblies, string queuePrefix = null,
            ILogger logger = null)
        {
            var localRequests = assemblies.SelectMany(a => a
                .GetTypes()
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.FullName != null && i.FullName.StartsWith("MediatR.IRequestHandler"))
                    .Select(i => i.GetGenericArguments()[0]).ToArray()
                ));
            options.SetAsLocalRequests(localRequests.ToArray, queuePrefix, logger);
            return options;
        }


        /// <summary>
        /// Infers local notifications for the specified <see cref="RouterOptions"/> object based on the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="options">The <see cref="RouterOptions"/> object.</param>
        /// <param name="assemblies">The collection of <see cref="Assembly"/> objects to infer local notifications from.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>
        /// The <see cref="RouterOptions"/> object with inferred local notifications set.
        /// </returns>
        public static RouterOptions InferLocalNotifications(this RouterOptions options, IEnumerable<Assembly> assemblies, string queuePrefix = null,
            ILogger logger = null)
        {
            var localNotifications = assemblies.SelectMany(a => a
                .GetTypes()
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.FullName != null && i.FullName.StartsWith("MediatR.INotificationHandler"))
                    .Select(i => i.GetGenericArguments()[0]).ToArray()
                ));

            options.SetAsLocalRequests(() => localNotifications, queuePrefix, logger);

            return options;
        }

        /// <summary>
        /// Sets the specified type of request as a local request in the given RouterOptions object.
        /// </summary>
        /// <typeparam name="T">The type of the request to set as local.</typeparam>
        /// <param name="options">The RouterOptions object to modify.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The modified RouterOptions object.</returns>
        public static RouterOptions SetAsLocalRequest<T>(this RouterOptions options, string queuePrefix = null, ILogger logger = null)
            where T : IBaseRequest
        {
            options.LocalRequests.Add(typeof(T));

            if (!string.IsNullOrWhiteSpace(queuePrefix) && !options.QueuePrefixes.ContainsKey(typeof(T).FullName))
            {
                options.QueuePrefixes.Add(typeof(T).FullName, queuePrefix);
                logger?.LogInformation($"Added prefix to request ${typeof(T).FullName}");
            }

            return options;
        }

        /// <summary>
        /// Listens for a notification and adds it to the local requests list in the RouterOptions instance. </summary> <typeparam name="T">The type of the notification to listen for. It must implement the INotification interface.</typeparam> <param name="options">The RouterOptions instance to add the notification to.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The updated RouterOptions instance with the notification added to the local requests list.</returns>
        /// /
        public static RouterOptions ListenForNotification<T>(this RouterOptions options, string queuePrefix = null, ILogger logger = null)
            where T : INotification
        {
            options.LocalRequests.Add(typeof(T));

            if (!string.IsNullOrWhiteSpace(queuePrefix) && !options.QueuePrefixes.ContainsKey(typeof(T).FullName))
            {
                options.QueuePrefixes.Add(typeof(T).FullName, queuePrefix);
                logger?.LogInformation($"Added prefix to request ${typeof(T).FullName}");
            }

            return options;
        }

        /// <summary>
        /// Sets the specified type as a remote request and adds it to the remote requests list in the <see cref="RouterOptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of the remote request. It must implement the <see cref="IBaseRequest"/> interface.</typeparam>
        /// <param name="options">The <see cref="RouterOptions"/> instance to modify.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The modified <see cref="RouterOptions"/> instance.</returns>
        public static RouterOptions SetAsRemoteRequest<T>(this RouterOptions options, string queuePrefix = null, ILogger logger = null)
            where T : IBaseRequest
        {
            options.RemoteRequests.Add(typeof(T));

            if (!string.IsNullOrWhiteSpace(queuePrefix) && !options.QueuePrefixes.ContainsKey(typeof(T).FullName))
            {
                options.QueuePrefixes.Add(typeof(T).FullName, queuePrefix);
                logger?.LogInformation($"Added prefix to request ${typeof(T).FullName}");
            }


            return options;
        }

        /// <summary>
        /// Adds selected types from the specified assemblies as local requests to the <see cref="RouterOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="RouterOptions"/> to modify.</param>
        /// <param name="assemblySelect">A function that selects the assemblies to retrieve types from.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The modified <see cref="RouterOptions"/>.</returns>
        public static RouterOptions SetAsLocalRequests(this RouterOptions options, Func<IEnumerable<Assembly>> assemblySelect, string queuePrefix = null,
            ILogger logger = null)
        {
            var types = (from a in assemblySelect()
                from t in a.GetTypes()
                where typeof(IBaseRequest).IsAssignableFrom(t) || typeof(INotification).IsAssignableFrom(t)
                select t).AsEnumerable();

            foreach (var t in types)
                options.LocalRequests.Add(t);

            if (!string.IsNullOrWhiteSpace(queuePrefix))
                foreach (var t in types)
                    if (!options.QueuePrefixes.ContainsKey(t.FullName))
                    {
                        options.QueuePrefixes.Add(t.FullName, queuePrefix);
                        logger?.LogInformation($"Added prefix to request ${t.FullName}");
                    }

            return options;
        }

        /// <summary>
        /// Sets the specified types as local requests in the <see cref="RouterOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="RouterOptions"/> object.</param>
        /// <param name="typesSelect">A function that returns an enumerable collection of types to be set as local requests.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The updated <see cref="RouterOptions"/> object.</returns>
        public static RouterOptions SetAsLocalRequests(this RouterOptions options, Func<IEnumerable<Type>> typesSelect, string queuePrefix = null,
            ILogger logger = null)
        {
            foreach (var t in typesSelect())
                options.LocalRequests.Add(t);

            if (!string.IsNullOrWhiteSpace(queuePrefix))
                foreach (var t in typesSelect())
                    if (!options.QueuePrefixes.ContainsKey(t.FullName))
                    {
                        options.QueuePrefixes.Add(t.FullName, queuePrefix);
                        logger?.LogInformation($"Added prefix to request ${t.FullName}");
                    }

            return options;
        }

        /// <summary>
        /// Sets the specified <paramref name="options"/> as remote requests.
        /// </summary>
        /// <param name="options">The <see cref="RouterOptions"/> to set as remote requests.</param>
        /// <param name="assemblySelect">The function to select the assemblies.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The updated <see cref="RouterOptions"/> with remote requests set.</returns>
        public static RouterOptions SetAsRemoteRequests(this RouterOptions options, Func<IEnumerable<Assembly>> assemblySelect, string queuePrefix = null,
            ILogger logger = null)
        {
            var types = (from a in assemblySelect()
                from t in a.GetTypes()
                where typeof(IBaseRequest).IsAssignableFrom(t) || typeof(INotification).IsAssignableFrom(t)
                select t).AsEnumerable();
            foreach (var t in types)
                options.RemoteRequests.Add(t);

            if (!string.IsNullOrWhiteSpace(queuePrefix))
                foreach (var t in types)
                    if (!options.QueuePrefixes.ContainsKey(t.FullName))
                    {
                        options.QueuePrefixes.Add(t.FullName, queuePrefix);
                        logger?.LogInformation($"Added prefix to request ${t.FullName}");
                    }

            return options;
        }

        /// <summary>
        /// Sets the Types as remote requests.
        /// </summary>
        /// <param name="options">The RouterOptions object.</param>
        /// <param name="typesSelect">The function that returns IEnumerable of Type objects.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for requests</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The modified RouterOptions object.</returns>
        public static RouterOptions SetAsRemoteRequests(this RouterOptions options, Func<IEnumerable<Type>> typesSelect, string queuePrefix = null,
            ILogger logger = null)
        {
            var types = typesSelect();
            if (!types.Any())
                logger?.LogWarning("SetAsRemoteRequests : No Requests classes found in assemblies");

            foreach (var t in types)
                options.RemoteRequests.Add(t);

            if (!string.IsNullOrWhiteSpace(queuePrefix))
                foreach (var t in types)
                    if (!options.QueuePrefixes.ContainsKey(t.FullName))
                    {
                        options.QueuePrefixes.Add(t.FullName, queuePrefix);
                        logger?.LogInformation($"Added prefix to request ${t.FullName}");
                    }

            return options;
        }

        /// <summary>
        /// Set a prefix for notifications queue name.
        /// </summary>
        /// <param name="options">The RouterOptions object.</param>
        /// <param name="typesSelect">The function that returns IEnumerable of Type objects.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for notification</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The modified RouterOptions object.</returns>
        public static RouterOptions SetNotificationPrefix(this RouterOptions options, Func<IEnumerable<Type>> typesSelect, string queuePrefix,
            ILogger logger = null)
        {
            var types = typesSelect().Where(t => typeof(INotification).IsAssignableFrom(t));
            if (!types.Any())
                logger?.LogWarning("SetNotificationPrefix : No Notification classes found in assemblies");

            if (!string.IsNullOrWhiteSpace(queuePrefix))
                foreach (var t in types)
                    if (!options.QueuePrefixes.ContainsKey(t.FullName))
                    {
                        options.QueuePrefixes.Add(t.FullName, queuePrefix);
                        logger?.LogInformation($"Added prefix to notification ${t.FullName}");
                    }

            return options;
        }

        /// <summary>
        /// Set a prefix for notifications queue name.
        /// </summary>
        /// <param name="options">The RouterOptions object.</param>
        /// <param name="assemblySelect">The function to select the assemblies.</param>
        /// <param name="queuePrefix">Prefix for Exchange and queues for notification</param>
        /// <param name="logger">Logger instance to allow information during configuration</param>
        /// <returns>The modified RouterOptions object.</returns>
        public static RouterOptions SetNotificationPrefix(this RouterOptions options, Func<IEnumerable<Assembly>> assemblySelect, string queuePrefix,
            ILogger logger = null)
        {
            var types = (from a in assemblySelect()
                from t in a.GetTypes()
                where typeof(INotification).IsAssignableFrom(t)
                select t).AsEnumerable();

            if (!types.Any())
                logger?.LogWarning("SetNotificationPrefix : No Notification classes found in assemblies");

            foreach (var t in types)
                if (!options.QueuePrefixes.ContainsKey(t.FullName))
                {
                    options.QueuePrefixes.Add(t.FullName, queuePrefix);
                    logger?.LogInformation($"Added prefix to notification ${t.FullName}");
                }

            return options;
        }

        /// <summary>
        /// Gets the queue name for the specified type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to append the queue name to (optional).</param>
        /// <returns>The queue name for the specified type.</returns>
        public static string TypeQueueName(this Type t, RouterOptions options, StringBuilder sb = null)
        {
            if (t.CustomAttributes.Any())
            {
                var attr = t.GetCustomAttribute<RoutingQueueNameAttribute>();
                if (attr != null) return $"{t.Namespace}.{attr.Name}".Replace(".", "_");
            }

            // var prefix = options.DefaultQueuePrefix;
            options.QueuePrefixes.TryGetValue(t.FullName, out var prefix);
            prefix = prefix ?? options.DefaultQueuePrefix;

            sb = sb ?? new StringBuilder();

            if (!string.IsNullOrWhiteSpace(prefix)) sb.Append($"{prefix}.");
            sb.Append($"{t.Namespace}.{t.Name}");

            if (t.GenericTypeArguments != null && t.GenericTypeArguments.Length > 0)
            {
                sb.Append("[");
                foreach (var ta in t.GenericTypeArguments)
                {
                    ta.TypeQueueName(options, sb);
                    sb.Append(",");
                }

                sb.Append("]");
            }

            return sb.ToString().Replace(",]", "]").Replace(".", "_");
        }

        public static int? QueueTimeout(this Type t)
        {
            if (t.CustomAttributes.Any())
            {
                var attr = t.GetCustomAttribute<RoutingQueueTimeoutAttribute>();
                if (attr != null) return attr.ConsumerTimeout;
            }

            return null;
        }
    }
}