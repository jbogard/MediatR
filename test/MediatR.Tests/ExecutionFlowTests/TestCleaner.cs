using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MediatR.ExceptionHandling;
using MediatR.ExceptionHandling.Request.Subscription;
using MediatR.ExceptionHandling.RequestResponse.Subscription;
using MediatR.Subscriptions;
using MediatR.Subscriptions.Notifications;
using MediatR.Subscriptions.Requests;
using MediatR.Subscriptions.StreamingRequests;

namespace MediatR.ExecutionFlowTests;

internal static class TestCleaner
{
    private static readonly FieldInfo subscriptionFactoryOpenGenericNotificationHandlerField = typeof(SubscriptionFactory)
        .GetField("genericNotificationHandlerType", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo subscriptionFactoryOpenGenericRequestHandlerField = typeof(SubscriptionFactory)
        .GetField("genericRequestHandlerType", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo subscriptionFactoryOpenGenericRequestResponseHandlerField = typeof(SubscriptionFactory)
        .GetField("genericRequestResponseHandlerType", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo subscriptionFactoryOpenGenericStreamRequestHandlerField = typeof(SubscriptionFactory)
        .GetField("genericStreamRequestHandlerType", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo exceptionHandlerFactoryRequestExceptionActionHandlerField = typeof(ExceptionHandlerFactory)
        .GetField("genericRequestExceptionActionHandler", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo exceptionHandlerFactoryRequestExceptionHandlerField = typeof(ExceptionHandlerFactory)
        .GetField("genericRequestExceptionHandler", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo exceptionHandlerFactoryRequestResponseExceptionActionHandlerField = typeof(ExceptionHandlerFactory)
        .GetField("genericRequestResponseExceptionActionHandler", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly FieldInfo exceptionHandlerFactoryRequestResponseExceptionHandlerField = typeof(ExceptionHandlerFactory)
        .GetField("genericRequestResponseExceptionHandler", BindingFlags.Static | BindingFlags.NonPublic)!;
    
    private static readonly FieldInfo[] mediatorConcurrentDictionaries = typeof(Mediator)
        .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
        .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
        .ToArray();

    private static readonly FieldInfo[] exceptionFactoryConcurrentDictionaries = typeof(ExceptionHandlerFactory)
        .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
        .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
        .ToArray();

    public static void CleanUp()
    {
        CleanUpStaticDictionaryCaches();
        ResetConfigurationFields();
    }

    private static void CleanUpStaticDictionaryCaches()
    {
        foreach (var mediatorConcurrentDictionary in mediatorConcurrentDictionaries)
        {
            var dictionary = mediatorConcurrentDictionary.GetValue(null);
            GetCleanMethod(mediatorConcurrentDictionary.FieldType).Invoke(dictionary, Array.Empty<object?>());
        }

        foreach (var exceptionFactoryConcurrentDictionary in exceptionFactoryConcurrentDictionaries)
        {
            var dictionary = exceptionFactoryConcurrentDictionary.GetValue(null);
            GetCleanMethod(exceptionFactoryConcurrentDictionary.FieldType).Invoke(dictionary, Array.Empty<object?>());
        }

        static MethodInfo GetCleanMethod(Type concurrentDictionaryType)
        {
            return concurrentDictionaryType.GetMethod(nameof(ConcurrentDictionary<dynamic, dynamic>.Clear))!;
        }
    }

    private static void ResetConfigurationFields()
    {
        subscriptionFactoryOpenGenericNotificationHandlerField.SetValue(null, typeof(TransientNotificationHandler<>));
        subscriptionFactoryOpenGenericRequestHandlerField.SetValue(null, typeof(TransientRequestHandler<>));
        subscriptionFactoryOpenGenericRequestResponseHandlerField.SetValue(null, typeof(TransientRequestResponseHandler<,>));
        subscriptionFactoryOpenGenericStreamRequestHandlerField.SetValue(null, typeof(TransientStreamRequestHandler<,>));

        exceptionHandlerFactoryRequestExceptionActionHandlerField.SetValue(null, typeof(TransientRequestExceptionActionHandler<,>));
        exceptionHandlerFactoryRequestExceptionHandlerField.SetValue(null, typeof(TransientRequestExceptionRequestHandler<,>));
        exceptionHandlerFactoryRequestResponseExceptionActionHandlerField.SetValue(null, typeof(TransientRequestResponseExceptionActionHandler<,,>));
        exceptionHandlerFactoryRequestResponseExceptionHandlerField.SetValue(null, typeof(TransientRequestResponseExceptionHandler<,,>));
    }
}