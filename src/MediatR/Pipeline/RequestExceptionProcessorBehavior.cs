namespace MediatR.Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestExceptionHandler{TRequest,TResponse,TException}"/> or <see cref="RequestExceptionHandler{TRequest,TResponse}"/> instances after an exception is thrown by the following pipeline steps
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ServiceFactory _serviceFactory;

        public RequestExceptionProcessorBehavior(ServiceFactory serviceFactory) => _serviceFactory = serviceFactory;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var state = new RequestExceptionHandlerState<TResponse>();
                Type exceptionType = null;

                while (!state.Handled && exceptionType != typeof(Exception))
                {
                    exceptionType = exceptionType == null ? exception.GetType() : exceptionType.BaseType;
                    var exceptionHandlers = GetExceptionHandlers(request, exceptionType, out MethodInfo handleMethod);

                    foreach (var exceptionHandler in exceptionHandlers)
                    {
                        await ((Task)handleMethod.Invoke(exceptionHandler, new object[] { request, exception, state, cancellationToken })).ConfigureAwait(false);

                        if (state.Handled)
                        {
                            break;
                        }
                    }
                }
                
                if (!state.Handled)
                {
                    throw;
                }

                return state.Response;
            }
        }

        private IList<object> GetExceptionHandlers(TRequest request, Type exceptionType, out MethodInfo handleMethodInfo)
        {
            var exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);
            var enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
            handleMethodInfo = exceptionHandlerInterfaceType.GetMethod(nameof(RequestExceptionHandler<TRequest, TResponse>.Handle));

            var exceptionHandlers = (IEnumerable<object>)_serviceFactory.Invoke(enumerableExceptionHandlerInterfaceType);

            return RemoveOverriddenHandlers(request, exceptionHandlers.ToList());
        }

        private IList<object> RemoveOverriddenHandlers(TRequest request, IList<object> exceptionHandlers)
        {
            if (exceptionHandlers.Count < 2)
            {
                return exceptionHandlers;
            }

            var requestObjectDetails = new ObjectDetails(request);
            var allExceptionHandlers = exceptionHandlers.Select(s => new ObjectDetails(s)).ToList();            

            var uniqueHandlersGroups = allExceptionHandlers.GroupBy(g => g.Name).ToList();
            if (uniqueHandlersGroups.Count == exceptionHandlers.Count)
            {
                return exceptionHandlers;
            }

            var uniqueHandlers = new List<object>();
            foreach (var uniqueHandlersGroup in uniqueHandlersGroups)
            {
                var sameHandlers = uniqueHandlersGroup.ToArray();
                Array.Sort(sameHandlers, requestObjectDetails);

                uniqueHandlers.Add(sameHandlers[0].Value);
            }

            return uniqueHandlers;
        }
    }

    internal class ObjectDetails : IComparer<ObjectDetails>
    {
        public string Name { get; private set; }

        public string AssemblyName { get; private set; }

        public string Location { get; private set; }

        public object Value { get; private set; }

        public ObjectDetails(object value)
        {
            Value = value;
            var exceptionHandlerType = value.GetType();

            Name = exceptionHandlerType.Name;
            AssemblyName = exceptionHandlerType.Assembly.GetName().Name;
            Location = exceptionHandlerType.Namespace.Replace($"{AssemblyName}.", string.Empty);
        }

        public int Compare(ObjectDetails x, ObjectDetails y)
        {
            var compareByAssemblyResult = CompareByAssembly(x, y);
            if (compareByAssemblyResult != null)
            {
                return compareByAssemblyResult.Value;
            }

            var compareByNamespaceResult = CompareByNamespace(x, y);
            if (compareByNamespaceResult != null)
            {
                return compareByNamespaceResult.Value;
            }

            return CompareByLocation(x, y);
        }

        /// <summary>
        /// Compare two objects according to current assembly
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>
        /// An object has a higher priority if it belongs to the current assembly and the other is not;
        /// If none of the objects belong to the current assembly, they can be considered equal;
        /// If both objects belong to the current assembly, they can't be compared only by this criterion.
        /// </returns>
        private int? CompareByAssembly(ObjectDetails x, ObjectDetails y)
        {
            if (x.AssemblyName == AssemblyName && y.AssemblyName != AssemblyName)
            {
                return -1;
            }
            else if (x.AssemblyName != AssemblyName && y.AssemblyName == AssemblyName)
            {
                return 1;
            }
            else if (x.AssemblyName != AssemblyName && y.AssemblyName != AssemblyName)
            {
                return 0;
            }

            return null;
        }

        /// <summary>
        /// Compare two objects according to current namespace
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>
        /// An object has a higher priority if it belongs to the current/child namespace and the other is not;
        /// If both objects belong to the current/child namespace, they can be considered equal;
        /// If none of the objects belong to the current/child namespace, they can't be compared by this criterion.
        /// </returns>
        private int? CompareByNamespace(ObjectDetails x, ObjectDetails y)
        {
            if (x.Location.StartsWith(Location) && !y.Location.StartsWith(Location))
            {
                return -1;
            }
            else if (!x.Location.StartsWith(Location) && y.Location.StartsWith(Location))
            {
                return 1;
            }
            else if (x.Location.StartsWith(Location) && y.Location.StartsWith(Location))
            {
                return 0;
            }

            return null;
        }

        /// <summary>
        /// Compare two objects according to location in the assembly
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns>
        /// An object has a higher priority if it location is part of the current location and the other is not;
        /// If both objects are part of the current location, the closest has higher priority;
        /// If none of the objects are part of the current location, they can be considered equal.
        /// </returns>
        private int CompareByLocation(ObjectDetails x, ObjectDetails y)
        {
            if (Location.StartsWith(x.Location) && !Location.StartsWith(y.Location))
            {
                return -1;
            }
            else if (!Location.StartsWith(x.Location) && Location.StartsWith(y.Location))
            {
                return 1;
            }
            else if (x.Location.Length > y.Location.Length)
            {
                return -1;
            }
            else if (x.Location.Length < y.Location.Length)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
