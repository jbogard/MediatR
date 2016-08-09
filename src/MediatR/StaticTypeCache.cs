using System;
using System.Collections.Concurrent;

namespace MediatR
{
    internal class StaticTypeCache : ITypeCache
    {
        private static readonly ConcurrentDictionary<Type, Type> GenericHandlerCache;
        private static readonly ConcurrentDictionary<Type, Type> WrapperHandlerCache;

        static StaticTypeCache() {
            GenericHandlerCache = new ConcurrentDictionary<Type, Type>();
            WrapperHandlerCache = new ConcurrentDictionary<Type, Type>();
        }

        public Type GetWrapperType<TWrapper, TResponse>(Type wrapperType, Type requestType) {
            return WrapperHandlerCache.GetOrAdd(requestType, wrapperType, (type, root) => root.MakeGenericType(type, typeof(TResponse)));
        }

        public Type GetGenericHandlerType<TWrapper, TResponse>(Type handlerType, Type requestType) {
            return GenericHandlerCache.GetOrAdd(requestType, handlerType, (type, root) => root.MakeGenericType(type, typeof(TResponse)));
        }

        public Type GetWrapperNotificationHandler<TWrapper>(Type wrapperType, Type notificationType) {
            return Cache<TWrapper>.WrapperHandlerCache.GetOrAdd(notificationType, wrapperType, (type, root) => root.MakeGenericType(type));
        }

        public Type GetGenericNotificationHandler<TWrapper>(Type handlerType, Type notificationType) {
            return GenericHandlerCache.GetOrAdd(notificationType, handlerType, (type, root) => root.MakeGenericType(type));
        }

        internal static class Cache<T>
        {
            internal static ConcurrentDictionary<Type, Type> WrapperHandlerCache { get; }

            static Cache()
            {
                WrapperHandlerCache = new ConcurrentDictionary<Type, Type>();
            }
        }
    }
}