using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MediatR.Registration
{
    public class DynamicServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider _rootProvider;
        private ServiceProvider _currentProvider;
        private readonly IServiceCollection _services;

        public DynamicServiceProvider(IServiceProvider rootProvider)
        {
            _rootProvider = rootProvider;
            _services = new ServiceCollection();
            _currentProvider = _services.BuildServiceProvider();
        }

        public IEnumerable<ServiceDescriptor> GetAllServiceDescriptors()
        {
            return _services; 
        }

        public void AddService(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var constructor = implementationType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (constructor != null)
            {
                var parameters = constructor.GetParameters();

                foreach (var parameter in parameters)
                {
                    // Check if the dependency is already registered
                    if (_currentProvider.GetService(parameter.ParameterType) != null)
                        continue;

                    // Attempt to resolve from the root provider
                    var dependency = _rootProvider.GetService(parameter.ParameterType);
                    if (dependency != null)
                    {   
                        // Dynamically register the dependency in the dynamic registry
                        _services.Add(new ServiceDescriptor(parameter.ParameterType, _ => dependency, lifetime));
                        RebuildProvider(); // Rebuild the internal provider to include the new service
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Unable to resolve dependency {parameter.ParameterType.FullName} for {serviceType.FullName}");
                    }
                }
            }
            _services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
            RebuildProvider();
        }

        public void AddService(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            // Add the service descriptor with the factory
            _services.Add(new ServiceDescriptor(serviceType, implementationFactory, lifetime));
            RebuildProvider();
        }
        
        public object? GetService(Type serviceType)
        {
            // Handle requests for IEnumerable<T>
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                // typeof(T) for IEnumerable<T>
                var elementType = serviceType.GenericTypeArguments[0];
                return CastToEnumerableOfType(GetServices(elementType), elementType);
            }

            // Try resolving from the current provider
            var service = _currentProvider.GetService(serviceType);
            if (service != null) return service;

            //fall back to root provider
            service = _rootProvider.GetService(serviceType);
            if(service != null) return service;

            //if not found in current or root then try to find the implementation and register it.
            if (serviceType.IsGenericType)
            {
                var genericArguments = serviceType.GetGenericArguments();
                var hasResponseType = genericArguments.Length > 1;
                var openInterface = hasResponseType ? typeof(IRequestHandler<,>) : typeof(IRequestHandler<>);
                 
                var requestType = genericArguments[0];                   
                Type? responseType = hasResponseType ? genericArguments[1] : null;
                    
                var implementationType = FindOpenGenericHandlerType(requestType, responseType);
                if(implementationType == null)
                    throw new InvalidOperationException($"No implementation found for {openInterface.FullName}");

                AddService(serviceType, implementationType);
            }

            //find the newly registered service
            service = _currentProvider.GetService(serviceType);
            if (service != null) return service;
            
            // Fallback to the root provider as a last resort
            return _rootProvider.GetService(serviceType);            
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            // Collect services from the dynamic provider
            var dynamicServices = _services
                .Where(d => d.ServiceType == serviceType)
                .Select(d => _currentProvider.GetService(d.ServiceType))
                .Where(s => s != null);

            // Collect services from the root provider
            var rootServices = _rootProvider
                .GetServices(serviceType)
                .Cast<object>();

            // Combine results and remove duplicates
            return dynamicServices.Concat(rootServices).Distinct()!;
        }

        private object CastToEnumerableOfType(IEnumerable<object> services, Type elementType)
        {
            var castMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.Cast))
                ?.MakeGenericMethod(elementType);

            var toListMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.ToList))
                ?.MakeGenericMethod(elementType);

            if (castMethod == null || toListMethod == null)
                throw new InvalidOperationException("Unable to cast services to the specified enumerable type.");

            var castedServices = castMethod.Invoke(null, new object[] { services });
            return toListMethod.Invoke(null, new[] { castedServices })!;
        }

        public Type? FindOpenGenericHandlerType(Type requestType, Type? responseType = null)
        {
            if (!requestType.IsGenericType)
                return null;

            // Define the target generic handler type
            var openHandlerType = responseType == null ? typeof(IRequestHandler<>) : typeof(IRequestHandler<,>);
            var genericArguments = responseType == null ? new Type[] { requestType } : new Type[] { requestType, responseType };
            var closedHandlerType = openHandlerType.MakeGenericType(genericArguments);

            // Get the current assembly
            var currentAssembly = Assembly.GetExecutingAssembly();

            // Get assemblies that reference the current assembly
            var consumingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetReferencedAssemblies()
                    .Any(reference => reference.FullName == currentAssembly.FullName));

            // Search for matching types
            var types = consumingAssemblies.SelectMany(x => x.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.IsGenericTypeDefinition)
                .ToList();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandlerType))
                {
                    // Check generic constraints
                    var concreteHandlerGenericArgs = type.GetGenericArguments();
                    var concreteHandlerConstraints = concreteHandlerGenericArgs.Select(x => x.GetGenericParameterConstraints());
                    var concreteRequestTypeGenericArgs = requestType.GetGenericArguments();
                    //var secondArgConstrants = genericArguments[1];

                    // Ensure the constraints are compatible
                    if (concreteHandlerConstraints
                        .Select((list, i) => new { List = list, Index = i })
                        .All(x => x.List.All(c => c.IsAssignableFrom(concreteRequestTypeGenericArgs[x.Index]))))
                    {
                        return type.MakeGenericType(concreteRequestTypeGenericArgs);
                    }
                }
            }

            return null; // No matching type found
        }

        private void RebuildProvider()
        {
            _currentProvider.Dispose();
            _currentProvider = _services.BuildServiceProvider();
        }

        public void Dispose()
        {
            _currentProvider.Dispose();
        }
    }
}
