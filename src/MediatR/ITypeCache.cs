using System;

namespace MediatR
{
    public interface ITypeCache
    {
        Type GetGenericHandlerType<T, T1>(Type handlerType, Type requestType);
        Type GetWrapperType<T, T1>(Type wrapperType, Type requestType);
        Type GetGenericNotificationHandler<T>(Type handlerType, Type notificationType);
        Type GetWrapperNotificationHandler<T>(Type wrapperType, Type notificationType);
    }
}