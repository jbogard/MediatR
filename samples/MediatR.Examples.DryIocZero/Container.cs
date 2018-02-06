/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/


namespace DryIocZero
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using ImTools;

    /// <summary>Minimal container to register service factory delegates and then resolve service from them.</summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
         Justification = "Does not contain any unmanaged resources.")]
    public sealed partial class Container : IRegistrator, IResolverContext
    {
        /// <summary>Creates container.</summary>
        public Container(IScopeContext scopeContext = null)
            : this(Ref.Of(ImHashMap<Type, FactoryDelegate>.Empty),
                Ref.Of(ImHashMap<Type, ImHashMap<object, FactoryDelegate>>.Empty),
                new Scope(name: "<singletons>"), scopeContext,
                currentScope: null, disposed: 0, parent: null, root: null)
        { }

        /// <summary>Full constructor with all state included.</summary>
        public Container(Ref<ImHashMap<Type, FactoryDelegate>> defaultFactories,
            Ref<ImHashMap<Type, ImHashMap<object, FactoryDelegate>>> keyedFactories,
            IScope singletonScope, IScopeContext scopeContext, IScope currentScope,
            int disposed, IResolverContext parent, IResolverContext root)
        {
            _defaultFactories = defaultFactories;
            _keyedFactories = keyedFactories;

            SingletonScope = singletonScope;
            ScopeContext = scopeContext;
            _currentScope = currentScope;

            _disposed = disposed;

            Parent = parent;
            Root = root;

            GetLastGeneratedFactoryID(ref _lastFactoryID);
        }

        private int _lastFactoryID;

        /// <summary>The unique factory ID, which may be used for runtime scoped registrations.</summary>
        public int GetNextFactoryID() => Interlocked.Increment(ref _lastFactoryID);

        #region IResolver

        partial void GetLastGeneratedFactoryID(ref int lastFactoryID);

        // todo: May be replace with TryResolveGenerated to accommodate for the possible null service
        partial void ResolveGenerated(ref object service, Type serviceType);

        /// <summary>Directly uses generated factories to resolve service. Or returns default if service does not have generated factory.</summary>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull", Justification = "Per design")]
        public object ResolveGeneratedOrGetDefault(Type serviceType)
        {
            object service = null;
            ResolveGenerated(ref service, serviceType);
            return service;
        }

        /// <summary>Resolves service from container and returns created service object.</summary>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Per design")]
        public object Resolve(Type serviceType, IfUnresolved ifUnresolved)
        {
            object service = null;
            if (_defaultFactories.Value.IsEmpty)
                ResolveGenerated(ref service, serviceType);
            return service 
                ?? ResolveDefaultFromRuntimeRegistrationsFirst(serviceType, ifUnresolved == IfUnresolved.ReturnDefault);
        }

        private object ResolveDefaultFromRuntimeRegistrationsFirst(Type serviceType, bool ifUnresolvedReturnDefault)
        {
            var factories = _defaultFactories.Value;
            var factory = factories.GetValueOrDefault(serviceType);

            object service = null;
            if (factory == null)
                ResolveGenerated(ref service, serviceType);
            else
                service = factory(this);

            return service ?? Throw.If(!ifUnresolvedReturnDefault,
                Error.UnableToResolveDefaultService, serviceType, factories.IsEmpty ? string.Empty : "non-");
        }

        // todo: May be replace with TryResolveGenerated to accommodate for the possible null service
        partial void ResolveGenerated(ref object service, 
            Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args);

        /// <summary>Directly uses generated factories to resolve service. Or returns default if service does not have generated factory.</summary>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull", Justification = "Per design")]
        public object ResolveGeneratedOrGetDefault(Type serviceType, object serviceKey)
        {
            object service = null;
            ResolveGenerated(ref service, serviceType, serviceKey,
                requiredServiceType: null, preRequestParent: null, args: null);
            return service;
        }

        /// <summary>Resolves service from container and returns created service object.</summary>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Per design")]
        object IResolver.Resolve(Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved, Type requiredServiceType, Request preResolveParent, object[] args)
        {
            // if no runtime registrations, then fast resolve from generated delegates
            object service = null;
            if (_keyedFactories.Value.IsEmpty)
            {
                if (serviceKey == null && requiredServiceType == null && preResolveParent == null && args == null)
                    ResolveGenerated(ref service, serviceType);
                else
                    ResolveGenerated(ref service, serviceType, serviceKey, requiredServiceType, preResolveParent, args);
            }

            // if not resolved from generated fallback to check runtime registrations first
            return service 
                ?? ResolveFromRuntimeRegistrationsFirst(serviceType, serviceKey, 
                   ifUnresolved == IfUnresolved.ReturnDefault, requiredServiceType, preResolveParent, args);
        }

        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Per design")]
        private object ResolveFromRuntimeRegistrationsFirst(Type serviceType, object serviceKey,
            bool ifUnresolvedReturnDefault, Type requiredServiceType, Request preResolveParent, object[] args)
        {
            serviceType = requiredServiceType ?? serviceType;

            // ignore the rest of arguments, e.g. preResolveParent when resolving the runtime substitutes
            if (serviceKey == null) 
                return ResolveDefaultFromRuntimeRegistrationsFirst(serviceType, ifUnresolvedReturnDefault);

            FactoryDelegate factory;
            var factories = _keyedFactories.Value.GetValueOrDefault(serviceType);
            if (factories != null && (factory = factories.GetValueOrDefault(serviceKey)) != null)
                return factory(this);

            // If not resolved from runtime registration then try resolve generated
            object service = null;
            ResolveGenerated(ref service, serviceType, 
                serviceKey, requiredServiceType, preResolveParent ?? Request.Empty, args);

            return service ?? Throw.If(!ifUnresolvedReturnDefault,
                Error.UnableToResolveKeyedService, serviceType, serviceKey, factories == null ? string.Empty : "non-");
        }
        
        partial void ResolveManyGenerated(ref IEnumerable<ResolveManyResult> services, Type serviceType);

        /// <summary>Resolves many generated only services. Ignores runtime registrations.</summary>
        public IEnumerable<ResolveManyResult> ResolveManyGeneratedOrGetEmpty(Type serviceType)
        {
            var manyGenerated = Enumerable.Empty<ResolveManyResult>();
            ResolveManyGenerated(ref manyGenerated, serviceType);
            return manyGenerated;
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey = null, 
            Type requiredServiceType = null, Request preResolveParent = null, object[] args = null)
        {
            serviceType = requiredServiceType ?? serviceType;

            var generatedFactories = Enumerable.Empty<ResolveManyResult>();
            ResolveManyGenerated(ref generatedFactories, serviceType);
            if (serviceKey != null)
                generatedFactories = generatedFactories.Where(x => serviceKey.Equals(x.ServiceKey));
            if (requiredServiceType != null)
                generatedFactories = generatedFactories.Where(x => requiredServiceType == x.RequiredServiceType);

            foreach (var generated in generatedFactories)
                yield return generated.FactoryDelegate(this);

            if (serviceKey == null)
            {
                var defaultFactory = _defaultFactories.Value.GetValueOrDefault(serviceType);
                if (defaultFactory != null)
                    yield return defaultFactory(this);
            }

            var keyedFactories = _keyedFactories.Value.GetValueOrDefault(serviceType);
            if (keyedFactories != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = keyedFactories.GetValueOrDefault(serviceKey);
                    if (factoryDelegate != null)
                        yield return factoryDelegate(this);
                }
                else
                {
                    var parent = preResolveParent?.Parent;
                    var compositeParentKey = parent != null && parent.ServiceType == serviceType
                        ? parent.ServiceKey
                        : null;

                    foreach (var resolution in keyedFactories.Enumerate())
                        if (compositeParentKey == null || !compositeParentKey.Equals(resolution.Key))
                            yield return resolution.Value(this);
                }
            }
        }

        #endregion

        #region IRegistrator

        /// <summary>Registers factory delegate with corresponding service type and service key.</summary>
        public void Register(Type serviceType, FactoryDelegate factoryDelegate, IReuse reuse, object serviceKey)
        {
            ThrowIfContainerDisposed();

            if (reuse != null)
                factoryDelegate = reuse.Apply(GetNextFactoryID(), factoryDelegate);

            if (serviceKey == null)
                _defaultFactories.Swap(it => it.AddOrUpdate(serviceType, factoryDelegate));
            else
                _keyedFactories.Swap(it => it.AddOrUpdate(serviceType,
                    (it.GetValueOrDefault(serviceType) ??
                     ImHashMap<object, FactoryDelegate>.Empty).AddOrUpdate(serviceKey, factoryDelegate)));
        }

        private Ref<ImHashMap<Type, FactoryDelegate>> _defaultFactories;
        private Ref<ImHashMap<Type, ImHashMap<object, FactoryDelegate>>> _keyedFactories;

        #endregion

        #region IResolverContext

        /// <summary>True if container is disposed.</summary>
        public bool IsDisposed => _disposed == 1;

        /// <summary>Parent context of the scoped context.</summary>
        public IResolverContext Parent { get; }

        /// <summary>The root context of the scoped context.</summary>
        public IResolverContext Root { get; }

        /// <summary>Scope containing container singletons.</summary>
        public IScope SingletonScope { get; }

        /// <summary>Current scope.</summary>
        public IScope CurrentScope =>
            ScopeContext == null ? _currentScope : ScopeContext.GetCurrentOrDefault();

        private readonly IScope _currentScope;

        /// <summary>Scope context or null of not necessary.</summary>
        public IScopeContext ScopeContext { get; }

        /// <summary>Specifies to wrap the scope in a resolver context.</summary>
        public IResolverContext WithCurrentScope(IScope scope)
        {
            ThrowIfContainerDisposed();
            return new Container(_defaultFactories, _keyedFactories, SingletonScope, ScopeContext,
                scope, _disposed, parent: this, root: Root ?? this);
        }

        /// <summary>Disposes opened scope or root container with Singletons and ScopeContext.</summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
             Justification = "Does not contain any unmanaged resources.")]
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            if (_currentScope != null)
            {
                _currentScope.Dispose();
                ScopeContext?.SetCurrent(scope => scope == _currentScope ? scope.Parent : scope);
            }
            else
            {
                _defaultFactories = Ref.Of(ImHashMap<Type, FactoryDelegate>.Empty);
                _keyedFactories = Ref.Of(ImHashMap<Type, ImHashMap<object, FactoryDelegate>>.Empty);

                SingletonScope.Dispose();
                ScopeContext?.Dispose();
            }
        }

        private int _disposed;
        private void ThrowIfContainerDisposed() =>
            Throw.If(_disposed == 1, Error.ContainerIsDisposed, this);

        #endregion

        /// <summary>Outputs scope info for open scope.</summary> 
        public override string ToString()
        {
            var scope = CurrentScope;
            var scopeStr = scope == null ? "container"
                    : ScopeContext != null
                        ? "ambient scoped container with scope " + scope
                        : "scoped container with scope " + scope;

            if (IsDisposed)
                scopeStr = "disposed " + scopeStr;

            return scopeStr;
        }
    }

    /// <summary>Identifies the service when resolving collection</summary>
    public struct ResolveManyResult
    {
        /// <summary>Factory, the required part</summary>
        public FactoryDelegate FactoryDelegate;

        /// <summary>Optional key</summary>
        public object ServiceKey;

        /// <summary>Optional required service type, can be an open-generic type.</summary>
        public Type RequiredServiceType;

        /// <summary>Constructs the struct.</summary>
        public static ResolveManyResult Of(FactoryDelegate factoryDelegate,
            object serviceKey = null, Type requiredServiceType = null) =>
            new ResolveManyResult
            {
                FactoryDelegate = factoryDelegate,
                ServiceKey = serviceKey,
                RequiredServiceType = requiredServiceType
            };
    }

    /// <summary>Should return value stored in scope.</summary>
    public delegate object CreateScopedValue();

    /// <summary>Lazy object storage that will create object with provided factory on first access,
    /// then will be returning the same object for subsequent access.</summary>
    public interface IScope : IDisposable
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        IScope Parent { get; }

        /// <summary>Optional name object associated with scope.</summary>
        object Name { get; }

        /// <summary>True if scope is disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>Looks up for stored item by id.</summary>
        bool TryGet(out object item, int id);

        /// <summary>Creates, stores, and returns stored disposable by id.</summary>
        object GetOrAdd(int id, CreateScopedValue createValue, int disposalIndex = -1);

        /// <summary>Tracked item will be disposed with the scope. 
        /// Smaller <paramref name="disposalIndex"/> will be disposed first.</summary>
        object TrackDisposable(object item, int disposalIndex = -1);

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        void SetOrAdd(int id, object item);
    }

    /// <summary>Declares minimal API for service resolution. 
    /// Resolve default and keyed is separated because of optimization for faster resolution of default service.</summary>
    public interface IResolver
    {
        /// <summary>Resolves default (non-keyed) service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        object Resolve(Type serviceType, IfUnresolved ifUnresolved);

        /// <summary>Resolves service instance from container.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">(optional) service key used for registering service.</param>
        /// <param name="ifUnresolved">(optional) Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">(optional) Registered or wrapped service type to use instead of <paramref name="serviceType"/>,
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="preResolveParent">(optional) Dependency chain info.</param>
        /// <param name="args">(optional) For Func{args} propagation through Resolve call boundaries.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> parameter.</returns>
        object Resolve(Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved, Type requiredServiceType, Request preResolveParent, object[] args);

        /// <summary>Resolves all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="preResolveParent">Dependency resolution path info.</param>
        /// <param name="args">(optional) For Func{args} propagation through Resolve call boundaries.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        IEnumerable<object> ResolveMany(Type serviceType, object serviceKey,
            Type requiredServiceType, Request preResolveParent, object[] args);
    }

    /// <summary>Extends IResolver to provide an access Scope hierarchy</summary>
    public interface IResolverContext : IResolver, IDisposable
    {
        /// <summary>True if container is disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>Parent context of the scoped context.</summary>
        IResolverContext Parent { get; }

        /// <summary>The root context of the scoped context.</summary>
        IResolverContext Root { get; }

        /// <summary>Singleton scope, always associated with root scope.</summary>
        IScope SingletonScope { get; }

        /// <summary>Current opened scope.</summary>
        IScope CurrentScope { get; }

        /// <summary>Optional scope context associated with container.</summary>
        IScopeContext ScopeContext { get; }

        /// <summary>Wraps the scope in resolver context (or container which implements the context).</summary>
        IResolverContext WithCurrentScope(IScope scope);
    }

    /// <summary>Provides APIs used by resolution generated factory delegates.</summary>
    public static class ResolverContext
    {
        /// <summary>Just a sugar that allow to get root or self container.</summary>
        public static IResolverContext RootOrSelf(this IResolverContext r) => r.Root ?? r;

        /// <summary>Provides access to the current scope.</summary>
        public static IScope GetCurrentScope(this IResolverContext r, bool throwIfNotFound) =>
            r.CurrentScope ?? (IScope)Throw.If(throwIfNotFound, Error.NoCurrentScope, r);

        /// <summary>Gets current scope matching the <paramref name="name"/></summary>
        public static IScope GetNamedScope(this IResolverContext r, object name, bool throwIfNotFound)
        {
            var scope = r.CurrentScope;
            if (scope == null)
                return (IScope)Throw.If(throwIfNotFound, Error.NoCurrentScope, r);

            if (name == null)
                return scope;

            var scopeName = name as IScopeName;
            if (scopeName != null)
            {
                for (; scope != null; scope = scope.Parent)
                    if (scopeName.Match(scope.Name))
                        return scope;
            }
            else
            {
                for (; scope != null; scope = scope.Parent)
                    if (ReferenceEquals(name, scope.Name) || name.Equals(scope.Name))
                        return scope;
            }

            return (IScope)Throw.If(throwIfNotFound, Error.NoMatchedScopeFound, name, r);
        }

        /// <summary>Allows to open scope with the provided name and specified tracking option.</summary>
        public static IResolverContext OpenScope(this IResolverContext r, object name = null, bool trackInParent = false)
        {
            var openedScope = r.ScopeContext == null
                ? new Scope(r.CurrentScope, name)
                : r.ScopeContext.SetCurrent(scope => new Scope(scope, name));
            
            if (trackInParent)
                (openedScope.Parent ?? r.SingletonScope).TrackDisposable(openedScope);

            return r.WithCurrentScope(openedScope);
        }
    }

    /// <summary>Service factory delegate which accepts resolver and resolution scope as parameters and should return service object.
    /// It is stateless because does not include state parameter as <c>DryIoc.FactoryDelegate</c>.</summary>
    public delegate object FactoryDelegate(IResolverContext r);

    /// <summary>Provides methods to register default or keyed factory delegates.</summary>
    public interface IRegistrator
    {
        /// <summary>Registers factory delegate with corresponding service type and service key.</summary>
        void Register(Type serviceType, FactoryDelegate factoryDelegate, IReuse reuse, object serviceKey);
    }

    /// <summary>Delegate to get new scope from old/existing current scope.</summary>
    /// <param name="oldScope">Old/existing scope to change.</param>
    /// <returns>New scope or old if do not want to change current scope.</returns>
    public delegate IScope SetCurrentScopeHandler(IScope oldScope);

    /// <summary>Provides ambient current scope and optionally scope storage for container, 
    /// examples are HttpContext storage, Execution context, Thread local.</summary>
    public interface IScopeContext : IDisposable
    {
        /// <summary>Name associated with context root scope - so the reuse may find scope context.</summary>
        string RootScopeName { get; }

        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        IScope GetCurrentOrDefault();

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and
        /// should return new current scope.</summary>
        /// <param name="setCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in <c>using (var newScope = ctx.SetCurrent(...))</c>.</returns>
        IScope SetCurrent(SetCurrentScopeHandler setCurrentScope);
    }

    /// <summary>Convenience extensions for registrations on top of delegate registrator.</summary>
    public static class Registrator
    {
        /// <summary>Registers user provided delegate to create the service</summary>
        public static void RegisterDelegate(this IRegistrator registrator,
            Type serviceType, Func<IResolverContext, object> factoryDelegate, 
            IReuse reuse = null, object serviceKey = null)
        {
            FactoryDelegate factory = context =>
            {
                var service = factoryDelegate(context);
                if (service != null)
                    Throw.If(!serviceType.GetTypeInfo().IsAssignableFrom(service.GetType().GetTypeInfo()),
                        Error.ProducedServiceIsNotAssignableToRequiredServiceType, service, serviceType);
                return service;
            };

            registrator.Register(serviceType, factory, reuse, serviceKey);
        }

        /// <summary>Registers user provided delegate to create the service</summary>
        public static void RegisterDelegate<TService>(this IRegistrator registrator,
            IReuse reuse, Func<IResolverContext, TService> factoryDelegate, object serviceKey = null) =>
            registrator.Register(typeof(TService), r => (TService)factoryDelegate(r), reuse, serviceKey);

        /// <summary>Registers user provided delegate to create the service</summary>
        public static void RegisterDelegate<TService>(this IRegistrator registrator,
            Func<IResolverContext, TService> factoryDelegate, object serviceKey = null) =>
            registrator.RegisterDelegate(null, factoryDelegate, serviceKey);

        /// <summary>Registers passed service instance.</summary>
        /// <typeparam name="TService">Service type, may be different from instance type.</typeparam>
        /// <param name="registrator">Registrator to register with.</param>
        /// <param name="instance">Externally managed service instance.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        public static void UseInstance<TService>(this IRegistrator registrator,
            TService instance, object serviceKey = null) => 
            registrator.RegisterDelegate(_ => instance, serviceKey);
    }

    /// <summary>Sugar to allow more simple resolve API</summary>
    public static class Resolver
    {
        /// <summary>Resolves service of specified service type.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType) =>
            resolver.Resolve(serviceType, IfUnresolved.Throw);

        /// <summary>Resolves service of specified service type allowing to return null for unresolved service.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType, bool ifUnresolvedReturnDefault) =>
            resolver.Resolve(serviceType, ifUnresolvedReturnDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw);

        /// <summary>Resolves service of specified service type.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType, object serviceKey,
            Type requiredServiceType = null, Request preResolveParent = null, object[] args = null) =>
            resolver.Resolve(serviceType, serviceKey, IfUnresolved.Throw, requiredServiceType, preResolveParent, args);

        /// <summary>Resolves service of specified service type.</summary>
        public static TService Resolve<TService>(this IResolver resolver) =>
            (TService)resolver.Resolve(typeof(TService), IfUnresolved.Throw);

        /// <summary>Resolves service of specified service type.</summary>
        public static TService Resolve<TService>(this IResolver resolver, bool ifUnresolvedReturnDefault) =>
            (TService)resolver.Resolve(typeof(TService), ifUnresolvedReturnDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw);

        /// <summary>Resolves service of specified service type.</summary>
        public static TService Resolve<TService>(this IResolver resolver, object serviceKey) =>
            (TService)resolver.Resolve(typeof(TService), serviceKey, IfUnresolved.Throw, 
                requiredServiceType: null, preResolveParent: Request.Empty, args: null);

        /// <summary>Resolves service of <typeparamref name="TService"/> type.</summary>
        public static TService Resolve<TService>(this IResolver resolver, Type requiredServiceType,
            bool ifUnresolvedReturnDefault = false, object serviceKey = null) =>
            (TService)(requiredServiceType == null && serviceKey == null
                ? resolver.Resolve(typeof(TService), 
                    ifUnresolvedReturnDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw)
                : resolver.Resolve(typeof(TService), serviceKey,
                    ifUnresolvedReturnDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw,
                    requiredServiceType, Request.Empty, null));

        /// <summary>Resolves collection of services of specified service type.</summary>
        public static IEnumerable<object> ResolveMany(this IResolver resolver, Type serviceType) =>
            resolver.ResolveMany(serviceType, null, null, Request.Empty, null);

        /// <summary>Resolves collection of services of specified service type.</summary>
        public static IEnumerable<object> ResolveMany(this IResolver resolver, Type serviceType, object serviceKey) =>
            resolver.ResolveMany(serviceType, serviceKey, null, Request.Empty, null);

        /// <summary>Resolves collection of services of specified service type.</summary>
        public static IEnumerable<TService> ResolveMany<TService>(this IResolver resolver) =>
            resolver.ResolveMany(typeof(TService)).Cast<TService>();

        /// <summary>Resolves collection of services of specified service type.</summary>
        public static IEnumerable<TService> ResolveMany<TService>(this IResolver resolver, object serviceKey) =>
            resolver.ResolveMany(typeof(TService), serviceKey).Cast<TService>();

        /// <summary>For given instance resolves and sets properties.</summary>
        /// <param name="resolver">Target resolver.</param>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="includeBase">(optional) By default only declared properties are injected, if set will add base properties too.</param>
        /// <param name="skipIfPropertyUnresolved">If true and property is unresolved, nothing would happen, no exception
        /// and property stays untouched</param>
        /// <returns>Instance with assigned properties.</returns>
        public static object InjectProperties(this IResolver resolver, object instance, bool includeBase = false,
            bool skipIfPropertyUnresolved = false)
        {
            var instanceType = instance.GetType();
            var ifUnresolved = skipIfPropertyUnresolved ? IfUnresolved.ReturnDefault : IfUnresolved.Throw;
            var properties = instanceType.Properties(includeBase);

            foreach (var propertyInfo in properties)
            {
                var value = resolver.Resolve(propertyInfo.PropertyType, ifUnresolved);
                if (value != null)
                    propertyInfo.SetValue(instance, value, null);
            }

            return instance;
        }
    }

    /// <summary>Scope implementation to hold and dispose stored <see cref="IDisposable"/> items.
    /// <c>lock</c> is used internally to ensure that object factory called only once.</summary>
    public sealed class Scope : IScope
    {
        /// <summary>Parent scope in scope stack. Null for the root scope.</summary>
        public IScope Parent { get; }

        /// <summary>Optional name associated with scope.</summary>
        public object Name { get; }

        /// <summary>True if scope is disposed.</summary>
        public bool IsDisposed => _disposed == 1;

        /// <summary>Creates scope with optional parent and name.</summary>
        public Scope(IScope parent = null, object name = null)
            : this(parent, name, ImMap<object>.Empty, ImMap<IDisposable>.Empty, int.MaxValue)
        { }

        private Scope(IScope parent, object name, ImMap<object> items,
            ImMap<IDisposable> disposables, int nextDisposalIndex)
        {
            Parent = parent;
            Name = name;
            _items = items;
            _disposables = disposables;
            _nextDisposalIndex = nextDisposalIndex;
        }

        internal static readonly MethodInfo GetOrAddMethod =
            typeof(IScope).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(IScope.GetOrAdd));

        /// <inheritdoc />
        public object GetOrAdd(int id, CreateScopedValue createValue, int disposalIndex = -1)
        {
            object value;
            return _items.TryFind(id, out value)
                ? value : TryGetOrAdd(id, createValue, disposalIndex);
        }

        private object TryGetOrAdd(int id, CreateScopedValue createValue, int disposalIndex = -1)
        {
            if (_disposed == 1)
                Throw.It(Error.ScopeIsDisposed, ToString());

            object item;
            lock (_locker)
            {
                if (_items.TryFind(id, out item)) // double check locking
                    return item;

                item = createValue();

                // Swap is required because if _items changed inside createValue, then we need to retry
                var items = _items;
                if (Interlocked.CompareExchange(ref _items, items.AddOrUpdate(id, item), items) != items)
                    Ref.Swap(ref _items, it => it.AddOrUpdate(id, item));
            }

            return TrackDisposable(item, disposalIndex);
        }

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at. Should be >= 0.</param> <param name="item">Value to set.</param>
        public void SetOrAdd(int id, object item)
        {
            if (_disposed == 1)
                Throw.It(Error.ScopeIsDisposed, ToString());
            var items = _items;

            // try to atomically replaced items with the one set item, if attempt failed then lock and replace
            if (Interlocked.CompareExchange(ref _items, items.AddOrUpdate(id, item), items) != items)
                lock (_locker)
                    _items = _items.AddOrUpdate(id, item);

            TrackDisposable(item);
        }

        /// <inheritdoc />
        public bool TryGet(out object item, int id)
        {
            if (_disposed == 1)
                Throw.It(Error.ScopeIsDisposed, ToString());
            return _items.TryFind(id, out item);
        }

        internal static readonly MethodInfo TrackDisposableMethod =
            typeof(IScope).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(IScope.TrackDisposable));

        /// <summary>Can be used to manually add service for disposal</summary>
        public object TrackDisposable(object item, int disposalIndex = -1)
        {
            if (item == this)
                return item;

            var disposable = item as IDisposable;
            if (disposable != null)
            {
                if (disposalIndex == -1)
                    disposalIndex = Interlocked.Decrement(ref _nextDisposalIndex);

                var it = _disposables;
                if (Interlocked.CompareExchange(ref _disposables, it.AddOrUpdate(disposalIndex, disposable), it) != it)
                    Ref.Swap(ref _disposables, _ => _.AddOrUpdate(disposalIndex, disposable));
            }
            return item;
        }

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and empties item storage.</summary>
        /// <remarks>If disposal throws exception, then it won't be propagated outside,
        /// so the rest of the items may proceed to be disposed.</remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            var disposables = _disposables;
            if (!disposables.IsEmpty)
                foreach (var disposable in disposables.Enumerate())
                {
                    // Ignoring disposing exception, as it is not important to proceed the disposal
                    try { disposable.Value.Dispose(); }
                    catch (Exception) { }
                }

            _disposables = ImMap<IDisposable>.Empty;

            _items = ImMap<object>.Empty;
        }

        /// <summary>Prints scope info (name and parent) to string for debug purposes.</summary>
        public override string ToString() =>
            (IsDisposed ? "disposed" : "") + "{"
            + (Name != null ? "Name=" + Name : "no name")
            + (Parent != null ? ", Parent=" + Parent : "")
            + "}";

        #region Implementation

        private ImMap<object> _items;
        private ImMap<IDisposable> _disposables;
        private int _nextDisposalIndex;
        private int _disposed;

        // todo: Improve performance by scaling lockers count with the items amount
        // Sync root is required to create object only once. The same reason as for Lazy<T>.
        private readonly object _locker = new object();

        #endregion
    }

    internal static class ScopedDisposableHandling
    {
        public static IDisposable TryUnwrapDisposable(object item)
        {
            var disposable = item as IDisposable;
            if (disposable != null)
                return disposable;

            // Unwrap WeakReference if item wrapped in it.
            var weakRefItem = item as WeakReference;
            if (weakRefItem != null)
                return weakRefItem.Target as IDisposable;

            return null;
        }

        public static void DisposeItem(object item)
        {
            var disposable = TryUnwrapDisposable(item);
            if (disposable != null)
            {
                try { disposable.Dispose(); }
                catch (Exception)
                {
                    // NOTE: Ignoring disposing exception, they not so important for program to proceed.
                }
            }
        }
    }

    /// <summary>List of error codes and messages.</summary>
    public static class Error
    {
        /// <summary>First error code to identify error range for other possible error code definitions.</summary>
        public static readonly int FirstErrorCode = 0;

        /// <summary>List of error messages indexed with code.</summary>
        public static readonly List<string> Messages = new List<string>(100);

#pragma warning disable 1591 // "Missing XML-comment"
        public static readonly int
            UnableToResolveDefaultService = Of(
                "Unable to resolve {0} from {1}empty runtime registrations and from generated factory delegates."),
            UnableToResolveKeyedService = Of(
                "Unable to resolve {0} with key [{1}] from {2}empty runtime registrations and from generated factory delegates."),
            NoCurrentScope = Of(
                "No current scope is available in {0}. Probably you are resolving from outside of scope."),
            NoMatchedScopeFound = Of(
                "Unable to find scope with name '{0}' in {1}."),
            ContainerIsDisposed = Of(
                "Container is disposed and not in operation state: {0}."),
            ScopeIsDisposed = Of(
                "Scope is disposed and scoped instances are no longer available."),
            ProducedServiceIsNotAssignableToRequiredServiceType = Of(
                "Service {0} produced by registered delegate or instance is not assignable to required service type {1}.");
#pragma warning restore 1591 // "Missing XML-comment"

        /// <summary>Generates new code for message.</summary>
        /// <param name="message">Message.</param> <returns>Code.</returns>
        public static int Of(string message)
        {
            Messages.Add(message);
            return FirstErrorCode + Messages.Count - 1;
        }
    }

    /// <summary>Zero container exception.</summary>
    [SuppressMessage("Microsoft.Usage",
         "CA2237:MarkISerializableTypesWithSerializable",
         Justification = "Not available in PCL")]
    public class ContainerException : InvalidOperationException
    {
        /// <summary>Error code.</summary>
        public int Error { get; }

        /// <summary>Creates exception.</summary>
        /// <param name="error">Code.</param> <param name="message">Message.</param>
        public ContainerException(int error, string message)
            : base(message)
        {
            Error = error;
        }
    }

    /// <summary>Simplifies throwing exceptions.</summary>
    public static class Throw
    {
        /// <summary>Just throws exception with specified error code.</summary>
        public static void It(int error, params object[] args)
        {
            var messageFormat = Error.Messages[error];
            var message = string.Format(messageFormat, args);
            throw new ContainerException(error, message);
        }

        /// <summary>Throws if condition is true.</summary>
        public static object If(bool condition, int error, params object[] args)
        {
            if (condition)
                It(error, args);
            return null;
        }
    }

    /// <summary>Called from generated code.</summary>
    public static class ThrowInGeneratedCode
    {
        /// <summary>Throws if object is null.</summary>
        public static object ThrowNewErrorIfNull(this object obj, string message)
        {
            if (obj == null)
                Throw.It(Error.Of(message));
            return obj;
        }
    }

    /// <summary>Abstracts way to match reuse and scope names</summary>
    public interface IScopeName
    {
        /// <summary>Does the job.</summary>
        bool Match(object scopeName);
    }

    /// <summary>Represents multiple names</summary>
    public sealed class CompositeScopeName : IScopeName
    {
        /// <summary>Wraps the multiple names</summary>
        public static CompositeScopeName Of(object[] names) => new CompositeScopeName(names);

        /// <summary>Matches all the name in a loop until first match is found, otherwise returns false.</summary>
        public bool Match(object scopeName)
        {
            for (int i = 0; i < _names.Length; i++)
            {
                var name = _names[i];
                if (name == scopeName)
                    return true;
                var aScopeName = name as IScopeName;
                if (aScopeName != null && aScopeName.Match(scopeName))
                    return true;
                if (scopeName != null && scopeName.Equals(name))
                    return true;
            }

            return false;
        }

        private CompositeScopeName(object[] names)
        {
            _names = names;
        }

        private readonly object[] _names;
    }

    /// <summary>Holds the name for the resolution scope.</summary>
    public sealed class ResolutionScopeName : IScopeName
    {
        /// <summary>Creates scope with specified service type and key</summary>
        public static ResolutionScopeName Of(Type serviceType = null, object serviceKey = null) =>
            new ResolutionScopeName(serviceType, serviceKey);

        /// <summary>Creates scope with specified service type and key.</summary>
        public static ResolutionScopeName Of<TService>(object serviceKey = null) =>
            new ResolutionScopeName(typeof(TService), serviceKey);

        /// <summary>Type of service opening the scope.</summary>
        public readonly Type ServiceType;

        /// <summary>Optional service key of service opening the scope.</summary>
        public readonly object ServiceKey;

        private ResolutionScopeName(Type serviceType, object serviceKey)
        {
            ServiceType = serviceType;
            ServiceKey = serviceKey;
        }

        /// <inheritdoc />
        public bool Match(object scopeName)
        {
            var name = scopeName as ResolutionScopeName;
            if (name == null)
                return false;

            return (ServiceType == null ||
                ServiceType.GetTypeInfo().IsAssignableFrom(name.ServiceType.GetTypeInfo()) ||
                ServiceType.GetTypeInfo().ContainsGenericParameters &&
                name.ServiceType.GetTypeInfo().IsGenericType && ServiceType.GetTypeInfo()
                    .IsAssignableFrom(name.ServiceType.GetGenericTypeDefinition().GetTypeInfo())) &&
                (ServiceKey == null || ServiceKey.Equals(name.ServiceKey));
        }

        /// <summary>String representation for easy debugging and understood error messages.</summary>
        public override string ToString()
        {
            var s = new StringBuilder(GetType().Name).Append(ServiceType.FullName ?? ServiceType.Name);
            if (ServiceKey != null)
                s.Append(',').Append(ServiceKey);
            return s.Append(")").ToString();
        }
    }

    /// <summary>Reuse goal is to locate or create scope where reused objects will be stored.</summary>
    /// <remarks><see cref="IReuse"/> implementers supposed to be stateless, and provide scope location behavior only.
    /// The reused service instances should be stored in scope(s).</remarks>
    public interface IReuse
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        int Lifespan { get; }

        /// <summary>Applies reuse to passed service creation factory.</summary>
        /// <param name="itemId">Reused item id, used to store and find item in scope.</param>
        /// <param name="factoryDelegate">Source factory</param> <returns>Transformed factory</returns>
        FactoryDelegate Apply(int itemId, FactoryDelegate factoryDelegate);
    }

    /// <summary>Specifies pre-defined reuse behaviors supported by container: 
    /// used when registering services into container with <see cref="Registrator"/> methods.</summary>
    public static class Reuse
    {
        /// <summary>Synonym for absence of reuse.</summary>
        public static readonly IReuse Transient = new TransientReuse();

        /// <summary>Specifies to store single service instance per <see cref="Container"/>.</summary>
        public static readonly IReuse Singleton = new SingletonReuse();

        /// <summary>Scoped reuse</summary>
        public static readonly IReuse Scoped = new CurrentScopeReuse();

        /// <summary>Scoped to the named scope.</summary>
        public static IReuse ScopedTo(object name) => new CurrentScopeReuse(name);

        /// <summary>Scoped to any scope with the one of specified names.</summary>
        public static IReuse ScopedTo(params object[] names) =>
            names.IsNullOrEmpty() ? Scoped :
            names.Length == 1 ? ScopedTo(names[0]) :
            new CurrentScopeReuse(CompositeScopeName.Of(names));

        /// <summary>Obsolete: please use <see cref="Scoped"/></summary>
        public static readonly IReuse InCurrentScope = new CurrentScopeReuse();

        /// <summary>Obsolete: please use <see cref="Scoped"/></summary>
        public static readonly IReuse InResolutionScope = Scoped;

        /// <summary>Obsolete: please use <see cref="ScopedTo(object)"/></summary>
        public static IReuse InCurrentNamedScope(object name = null) =>
            name == null ? InCurrentScope : new CurrentScopeReuse(name);

        /// <summary>Obsolete: please use <see cref="ScopedTo(object)"/></summary>
        public static IReuse InResolutionScopeOf(Type assignableFromServiceType = null, object serviceKey = null) =>
            assignableFromServiceType == null && serviceKey == null ? InResolutionScope
                : ScopedTo(ResolutionScopeName.Of(assignableFromServiceType, serviceKey));

        /// <summary>Obsolete: please use <see cref="ScopedTo(object)"/></summary>
        public static IReuse InResolutionScopeOf<TAssignableFromServiceType>(object serviceKey = null) =>
            ScopedTo(ResolutionScopeName.Of<TAssignableFromServiceType>(serviceKey));

        /// <summary>Special name that by convention recognized by <see cref="InWebRequest"/>.</summary>
        public static readonly string WebRequestScopeName = "WebRequestScopeName";

        /// <summary>Web request is just convention for reuse in <see cref="InCurrentNamedScope"/> with special name <see cref="WebRequestScopeName"/>.</summary>
        public static readonly IReuse InWebRequest = ScopedTo(WebRequestScopeName);
    }

    /// <summary>Transient reuse is for completeness, means no reuse.</summary>
    public sealed class TransientReuse : IReuse
    {
        /// <summary>Value relative to other reuses lifespan value.</summary>
        public int Lifespan => 0;

        /// <summary>Returns the input factory as-is. No reuse is applied.</summary>
        public FactoryDelegate Apply(int itemId, FactoryDelegate factoryDelegate) => factoryDelegate;
    }

    /// <summary>Singleton reuse.</summary>
    public sealed class SingletonReuse : IReuse
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan => 1000;

        /// <summary>Before invoking the delegates looks-up for instance in scope.</summary>
        public FactoryDelegate Apply(int itemId, FactoryDelegate factoryDelegate) =>
            r => r.SingletonScope.GetOrAdd(itemId, () => factoryDelegate(r));
    }

    /// <summary>Scoped reuse.</summary>
    public sealed class CurrentScopeReuse : IReuse
    {
        /// <summary>Name to find current scope or parent with equal name.</summary>
        public readonly object Name;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan => 100;

        /// <summary>Creates reuse optionally specifying its name.</summary> 
        public CurrentScopeReuse(object name = null)
        {
            Name = name;
        }

        /// <inheritdoc />
        public FactoryDelegate Apply(int itemId, FactoryDelegate factoryDelegate)
        {
            if (Name == null)
                return r => r.GetCurrentScope(true).GetOrAdd(itemId, () => factoryDelegate(r));
            return r => r.GetNamedScope(Name, true).GetOrAdd(itemId, () => factoryDelegate(r));
        }

        internal static object TrackScopedOrSingleton(IResolverContext r, object item) =>
            (r.CurrentScope ?? r.SingletonScope).TrackDisposable(item);

        internal static object GetScopedOrSingleton(IResolverContext r,
            int id, CreateScopedValue createValue, int disposalIndex) =>
            (r.CurrentScope ?? r.SingletonScope).GetOrAdd(id, createValue, disposalIndex);

        internal static object GetScoped(IResolverContext r,
            bool throwIfNoScope, int id, CreateScopedValue createValue, int disposalIndex = -1) =>
            r.GetCurrentScope(throwIfNoScope)?.GetOrAdd(id, createValue, disposalIndex);

        internal static object GetNameScoped(IResolverContext r,
            object scopeName, bool throwIfNoScope, int id, CreateScopedValue createValue, int disposalIndex = -1) =>
            r.GetNamedScope(scopeName, throwIfNoScope)?.GetOrAdd(id, createValue, disposalIndex);

        internal static object TrackScoped(IResolverContext r, bool throwIfNoScope, object item) =>
            r.GetCurrentScope(throwIfNoScope)?.TrackDisposable(item);

        internal static object TrackNameScoped(IResolverContext r,
            object scopeName, bool throwIfNoScope, object item) =>
            r.GetNamedScope(scopeName, throwIfNoScope)?.TrackDisposable(item);
    }

    /// <summary>Stored check results of two kinds: inherited down dependency chain and not.</summary>
    [Flags]
    public enum RequestFlags
    {
        /// <summary>Not inherited</summary>
        TracksTransientDisposable = 1 << 1,

        /// <summary>Not inherited</summary>
        IsServiceCollection = 1 << 2,

        /// <summary>Inherited</summary>
        IsSingletonOrDependencyOfSingleton = 1 << 3,

        /// <summary>Inherited</summary>
        IsWrappedInFunc = 1 << 4,

        /// <summary>Indicates that the request the one from Resolve call.</summary>
        IsResolutionCall = 1 << 5,

        /// <summary>Non inherited</summary>
        OpensResolutionScope = 1 << 6,

        /// <summary>Non inherited</summary>
        StopRecursiveDependencyCheck = 1 << 7
    }

    /// <summary>Type of services supported by Container.</summary>
    public enum FactoryType
    {
        /// <summary>(default) Defines normal service factory</summary>
        Service,
        /// <summary>Defines decorator factory</summary>
        Decorator,
        /// <summary>Defines wrapper factory.</summary>
        Wrapper
    }

    /// <summary>Policy to handle unresolved service.</summary>
    public enum IfUnresolved
    {
        /// <summary>If service is unresolved for whatever means, it will throw the respective exception.</summary>
        Throw,
        /// <summary>If service is unresolved for whatever means, it will return default(serviceType) value.</summary>
        ReturnDefault,
        /// <summary>If service is not registered, then it will return default, for other errors it will throw.</summary>
        ReturnDefaultIfNotRegistered,
    }

    /// <summary>Dependency request path information.</summary>
    public sealed class Request : IEnumerable<Request>
    {
        /// <summary>Represents empty info.</summary>
        public static readonly Request Empty = new Request();

        /// <summary>Represents an empty info and indicates an open resolution scope.</summary>
        public static readonly Request EmptyOpensResolutionScope = new Request(opensResolutionScope: true);

        /// <summary>Returns true for an empty request.</summary>
        public bool IsEmpty => ServiceType == null;

        /// <summary>Returns true if request is the first in a chain.</summary>
        public bool IsResolutionRoot => !IsEmpty && DirectParent.IsEmpty;

        /// <summary>Parent request or null for root resolution request.</summary>
        public readonly Request DirectParent;

        /// <summary>Returns service parent skipping wrappers if any.
        /// To get direct parent use <see cref="DirectParent"/>.</summary>
        public Request Parent
        {
            get
            {
                if (IsEmpty)
                    return Empty;
                var p = DirectParent;
                while (!p.IsEmpty && p.FactoryType == FactoryType.Wrapper)
                    p = p.DirectParent;
                return p;
            }
        }

        /// <summary>Asked service type.</summary>
        public readonly Type ServiceType;

        /// <summary>Required service type if specified.</summary>
        public readonly Type RequiredServiceType;

        /// <summary>Optional service key.</summary>
        public readonly object ServiceKey;

        /// <summary>Metadata key to find in metadata dictionary in resolved service.</summary>
        public readonly string MetadataKey;

        /// <summary>Metadata value to find in resolved service.</summary>
        public readonly object Metadata;

        /// <summary>Policy to deal with unresolved request.</summary>
        public readonly IfUnresolved IfUnresolved;

        /// <summary>Resolved factory ID, used to identify applied decorator.</summary>
        public readonly int FactoryID;

        /// <summary>False for Decorators and Wrappers.</summary>
        public readonly FactoryType FactoryType;

        /// <summary>Implementation type.</summary>
        public readonly Type ImplementationType;

        /// <summary>Service reuse.</summary>
        public readonly IReuse Reuse;

        /// <summary>Relative number representing reuse lifespan.</summary>
        public int ReuseLifespan => Reuse == null ? 0 : Reuse.Lifespan;

        /// <summary><see cref="RequestFlags"/>.</summary>
        public readonly RequestFlags Flags;

        /// <summary>Decorated factory ID for decorator request</summary>
        public readonly int DecoratedFactoryID;

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public Request Push(Type serviceType, int factoryID, Type implementationType, IReuse reuse) =>
            Push(serviceType, null, null, null, null, IfUnresolved.Throw,
                factoryID, FactoryType.Service, implementationType, reuse, default(RequestFlags), 0);

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public Request Push(Type serviceType, Type requiredServiceType, object serviceKey,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags) =>
            Push(serviceType, requiredServiceType, serviceKey, null, null, IfUnresolved.Throw,
                factoryID, factoryType, implementationType, reuse, flags, 0);

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public Request Push(Type serviceType, Type requiredServiceType, object serviceKey, IfUnresolved ifUnresolved,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags,
            int decoratedFactoryID) =>
            Push(serviceType, requiredServiceType, serviceKey, null, null, ifUnresolved,
                factoryID, factoryType, implementationType, reuse, flags, decoratedFactoryID);

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public Request Push(Type serviceType, Type requiredServiceType, object serviceKey, string metadataKey, object metadata, IfUnresolved ifUnresolved,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags,
            int decoratedFactoryID) =>
            new Request(serviceType, requiredServiceType, serviceKey, metadataKey, metadata, ifUnresolved,
                factoryID, factoryType, implementationType, reuse, flags, this, decoratedFactoryID);

        /// <summary>Obsolete: now request is directly implements the <see cref="IEnumerable{T}"/>.</summary>
        public IEnumerable<Request> Enumerate() => this;

        /// <summary>Returns all non-empty requests starting from the current request and ending with the root parent.
        /// Returns empty sequence for an empty request.</summary>
        public IEnumerator<Request> GetEnumerator()
        {
            for (var i = this; !i.IsEmpty; i = i.DirectParent)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Prints request with all its parents to string.</summary> <returns>The string.</returns>
        public override string ToString()
        {
            if (IsEmpty)
                return "{empty}";

            var s = new StringBuilder();

            if (FactoryType != FactoryType.Service)
                s.Append(FactoryType.ToString().ToLower()).Append(' ');

            if (ImplementationType != null && ImplementationType != ServiceType)
                s.Append(ImplementationType).Append(": ");

            s.Append(ServiceType);

            if (RequiredServiceType != null)
                s.Append(" with RequiredServiceType=").Append(RequiredServiceType);

            if (ServiceKey != null)
                s.Append(" with ServiceKey=").Append('{').Append(ServiceKey).Append('}');

            if (MetadataKey != null || Metadata != null)
                s.Append(" with Metadata=").Append(MetadataKey.Pair(Metadata));

            if (IfUnresolved != IfUnresolved.Throw)
                s.Append(" if unresolved ").Append(Enum.GetName(typeof(IfUnresolved), IfUnresolved));

            if (ReuseLifespan != 0)
                s.Append(" with ReuseLifespan=").Append(ReuseLifespan);

            if (!DirectParent.IsEmpty)
                s.AppendLine().Append("  in ").Append(DirectParent);

            return s.ToString();
        }

        /// <summary>Returns true if request info and passed object are equal, and their parents recursively are equal.</summary>
        public override bool Equals(object obj) =>
            Equals(obj as Request);

        /// <summary>Returns true if request info and passed info are equal, and their parents recursively are equal.</summary>
        public bool Equals(Request other) =>
            other != null && EqualsWithoutParent(other)
            && (DirectParent == null && other.DirectParent == null
                || (DirectParent != null && DirectParent.EqualsWithoutParent(other.DirectParent)));

        /// <summary>Compares info's regarding properties but not their parents.</summary>
        public bool EqualsWithoutParent(Request other) =>
            other.ServiceType == ServiceType

            && other.Flags == Flags

            && other.RequiredServiceType == RequiredServiceType
            && other.IfUnresolved == IfUnresolved
            && Equals(other.ServiceKey, ServiceKey)
            && other.MetadataKey == MetadataKey
            && Equals(other.Metadata, Metadata)

            && other.FactoryType == FactoryType
            && other.ImplementationType == ImplementationType
            && other.ReuseLifespan == ReuseLifespan;

        /// <summary>Calculates the combined hash code based on factory IDs.</summary>
        public override int GetHashCode()
        {
            if (IsEmpty)
                return 0;

            var hash = FactoryID;
            var parent = DirectParent;
            while (!parent.IsEmpty)
            {
                hash = CombineHashCodes(hash, parent.FactoryID);
                parent = parent.DirectParent;
            }

            return hash;
        }

        private Request(bool opensResolutionScope = false)
        {
            if (opensResolutionScope)
                Flags = RequestFlags.OpensResolutionScope;
        }

        private Request(
            Type serviceType, Type requiredServiceType, object serviceKey,
            string metadataKey, object metadata, IfUnresolved ifUnresolved,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse,
            RequestFlags flags, Request directParent,
            int decorateFactoryID)
        {
            DirectParent = directParent;

            // Service info:
            ServiceType = serviceType;
            RequiredServiceType = requiredServiceType;
            ServiceKey = serviceKey;
            MetadataKey = metadataKey;
            Metadata = metadata;
            IfUnresolved = ifUnresolved;

            // Implementation info:
            FactoryID = factoryID;
            FactoryType = factoryType;
            ImplementationType = implementationType;
            Reuse = reuse;

            DecoratedFactoryID = decorateFactoryID;

            Flags = flags;
        }

        // Inspired by System.Tuple.CombineHashCodes
        private static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                return (h1 << 5) + h1 ^ h2;
            }
        }
    }

    /// <summary>Used to represent multiple default service keys. 
    /// Exposes <see cref="RegistrationOrder"/> to determine order of service added.</summary>
    public sealed class DefaultKey
    {
        /// <summary>Default value.</summary>
        public static readonly DefaultKey Value = new DefaultKey(0);

        /// <summary>Allows to determine service registration order.</summary>
        public readonly int RegistrationOrder;

        /// <summary>Returns the default key with specified registration order.</summary>
        public static DefaultKey Of(int registrationOrder)
        {
            if (registrationOrder < _keyPool.Length)
                return _keyPool[registrationOrder];

            var nextKey = new DefaultKey(registrationOrder);
            if (registrationOrder == _keyPool.Length)
                _keyPool = _keyPool.AppendOrUpdate(nextKey);
            return nextKey;
        }

        /// <summary>Returns next default key with increased <see cref="RegistrationOrder"/>.</summary>
        public DefaultKey Next() => Of(RegistrationOrder + 1);

        /// <summary>Compares keys based on registration order.</summary>
        public override bool Equals(object key) => 
            key == null || (key as DefaultKey)?.RegistrationOrder == RegistrationOrder;

        /// <summary>Returns registration order as hash.</summary> <returns>Hash code.</returns>
        public override int GetHashCode() => RegistrationOrder;

        /// <summary>Prints registration order to string.</summary> <returns>Printed string.</returns>
        public override string ToString() => GetType().Name + ".Of(" + RegistrationOrder + ")";

        #region Implementation

        private static DefaultKey[] _keyPool = { Value };

        private DefaultKey(int registrationOrder)
        {
            RegistrationOrder = registrationOrder;
        }

        #endregion
    }

    internal sealed class HiddenDisposable
    {
        public readonly object Value;
        public HiddenDisposable(object value)
        {
            Value = value;
        }
    }

    /// <summary>Custom exclude from test code coverage attribute for portability.</summary>
    public sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        /// <summary>Optional reason of why the marked code is excluded from coverage.</summary>
        public readonly string Reason;

        /// <summary>Creates attribute with optional reason message.</summary> <param name="reason"></param>
        public ExcludeFromCodeCoverageAttribute(string reason = null)
        {
            Reason = reason;
        }
    }

    /// <summary>Helper and portability extensions to Reflection.</summary>
    public static class ReflectionTools
    {
        /// <summary>Returns specific type members. Does not look into base class by default.
        /// Specific members are returned by <paramref name="getMembers"/> delegate.</summary>
        public static IEnumerable<TMember> Members<TMember>(this Type type,
            Func<TypeInfo, IEnumerable<TMember>> getMembers,
            bool includeBase = false)
        {
            var typeInfo = type.GetTypeInfo();
            var members = getMembers(typeInfo);
            if (!includeBase)
                return members;
            var baseType = typeInfo.BaseType;
            return baseType == null || baseType == typeof(object)
                ? members
                : members.Concat(baseType.Members(getMembers, true));
        }

        /// <summary>Properties only.</summary>
        public static IEnumerable<PropertyInfo> Properties(this Type type, bool includeBase = false) => 
            type.Members(t => t.DeclaredProperties, includeBase: true);
    }
}

namespace DryIoc
{
    static partial class Portable
    {
        // ReSharper disable once UnusedMember.Local
        static partial void GetCurrentManagedThreadID(ref int threadID);
    }
}
