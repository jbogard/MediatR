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

/*
========================================================================================================
NOTE: The code below is generated automatically at compile-time and not supposed to be changed by hand.
========================================================================================================
There are 7 generation issues (may be not an error dependent on context):

The issues with run-time registrations may be solved by `container.RegisterPlaceholder<T>()` 
in Registrations.ttinclude. Then you can replace placeholders using `DryIocZero.Container.Register`
at runtime.

--------------------------------------------------------------------------------------------------------
1. MediatR.Pipeline.IRequestPreProcessor<>
Error: Resolving open-generic service type is not possible for type: MediatR.Pipeline.IRequestPreProcessor<>.
2. MediatR.Pipeline.IRequestPostProcessor<,> {ServiceKey=DefaultKey(0)}
Error: Resolving open-generic service type is not possible for type: MediatR.Pipeline.IRequestPostProcessor<,>.
3. MediatR.Pipeline.IRequestPostProcessor<,> {ServiceKey=DefaultKey(1)}
Error: Resolving open-generic service type is not possible for type: MediatR.Pipeline.IRequestPostProcessor<,>.
4. MediatR.IPipelineBehavior<,> {ServiceKey=DefaultKey(0)}
Error: Resolving open-generic service type is not possible for type: MediatR.IPipelineBehavior<,>.
5. MediatR.IPipelineBehavior<,> {ServiceKey=DefaultKey(1)}
Error: Resolving open-generic service type is not possible for type: MediatR.IPipelineBehavior<,>.
6. MediatR.IPipelineBehavior<,> {ServiceKey=DefaultKey(2)}
Error: Resolving open-generic service type is not possible for type: MediatR.IPipelineBehavior<,>.
7. MediatR.INotificationHandler<>
Error: Resolving open-generic service type is not possible for type: MediatR.INotificationHandler<>.
*/

using System;
using System.Linq; // for Enumerable.Cast method required by LazyEnumerable<T>
using System.Collections.Generic;
using System.Threading;
using ImTools;
using static DryIocZero.ResolveManyResult;

namespace DryIocZero
{
    partial class Container
    {
        [ExcludeFromCodeCoverage]
        partial void GetLastGeneratedFactoryID(ref int lastFactoryID)
        {
            lastFactoryID = 67; // generated: equals to last used Factory.FactoryID 
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType)
        {
            if (serviceType == typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>))
                service = Get0_RequestProcessor(this);

            else
            if (serviceType == typeof(MediatR.IRequest))
                service = Get3_IRequest(this);

            else
            if (serviceType == typeof(MediatR.IRequest<MediatR.Unit>))
                service = Get4_IRequest(this);

            else
            if (serviceType == typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>))
                service = Get5_INotificationHandler(this);

            else
            if (serviceType == typeof(MediatR.IRequestHandler<MediatR.Examples.Ping, MediatR.Examples.Pong>))
                service = Get6_IRequestHandler(this);

            else
            if (serviceType == typeof(MediatR.INotificationHandler<MediatR.INotification>))
                service = Get7_INotificationHandler(this);

            else
            if (serviceType == typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>))
                service = Get8_RequestProcessor(this);

            else
            if (serviceType == typeof(MediatR.IMediator))
                service = Get9_IMediator(this);

            else
            if (serviceType == typeof(MediatR.IRequest<MediatR.Examples.Pong>))
                service = Get10_IRequest(this);

            else
            if (serviceType == typeof(MediatR.IRequestHandler<MediatR.Examples.Jing>))
                service = Get11_IRequestHandler(this);

            else
            if (serviceType == typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>))
                service = Get14_NotificationProcessor(this);

            else
            if (serviceType == typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>))
                service = Get15_NotificationProcessor(this);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service,
            Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
        {
            if (serviceType == typeof(MediatR.INotification)) 
            {
                if (DefaultKey.Of(0).Equals(serviceKey))
                    service = Get1_INotification(this);

                else
                if (DefaultKey.Of(1).Equals(serviceKey))
                    service = Get2_INotification(this);
            }

            else
            if (serviceType == typeof(MediatR.IBaseRequest)) 
            {
                if (DefaultKey.Of(0).Equals(serviceKey))
                    service = Get12_IBaseRequest(this);

                else
                if (DefaultKey.Of(1).Equals(serviceKey))
                    service = Get13_IBaseRequest(this);
            }

            else
            if (serviceType == typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>)) 
            {
                if (DefaultKey.Of(0).Equals(serviceKey))
                    service = Get16_INotificationHandler(this);

                else
                if (DefaultKey.Of(1).Equals(serviceKey))
                    service = Get17_INotificationHandler(this);
            }
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveManyGenerated(ref IEnumerable<ResolveManyResult> services, Type serviceType) 
        {
            services = ResolveManyGenerated(serviceType);
        }

        [ExcludeFromCodeCoverage]
        private IEnumerable<ResolveManyResult> ResolveManyGenerated(Type serviceType)
        {
            if (serviceType == typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>))
            {
                yield return Of(Get0_RequestProcessor, typeof(MediatR.Pipeline.RequestProcessor<,>));
            }

            if (serviceType == typeof(MediatR.INotification))
            {
                yield return Of(Get1_INotification, DefaultKey.Of(0));
                yield return Of(Get2_INotification, DefaultKey.Of(1));
            }

            if (serviceType == typeof(MediatR.IRequest))
            {
                yield return Of(Get3_IRequest);
            }

            if (serviceType == typeof(MediatR.IRequest<MediatR.Unit>))
            {
                yield return Of(Get4_IRequest);
            }

            if (serviceType == typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>))
            {
                yield return Of(Get5_INotificationHandler);
                yield return Of(Get7_INotificationHandler); // co-variant
            }

            if (serviceType == typeof(MediatR.IRequestHandler<MediatR.Examples.Ping, MediatR.Examples.Pong>))
            {
                yield return Of(Get6_IRequestHandler);
            }

            if (serviceType == typeof(MediatR.INotificationHandler<MediatR.INotification>))
            {
                yield return Of(Get7_INotificationHandler);
            }

            if (serviceType == typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>))
            {
                yield return Of(Get8_RequestProcessor, typeof(MediatR.Pipeline.RequestProcessor<>));
            }

            if (serviceType == typeof(MediatR.IMediator))
            {
                yield return Of(Get9_IMediator);
            }

            if (serviceType == typeof(MediatR.IRequest<MediatR.Examples.Pong>))
            {
                yield return Of(Get10_IRequest);
            }

            if (serviceType == typeof(MediatR.IRequestHandler<MediatR.Examples.Jing>))
            {
                yield return Of(Get11_IRequestHandler);
            }

            if (serviceType == typeof(MediatR.IBaseRequest))
            {
                yield return Of(Get12_IBaseRequest, DefaultKey.Of(0));
                yield return Of(Get13_IBaseRequest, DefaultKey.Of(1));
            }

            if (serviceType == typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>))
            {
                yield return Of(Get14_NotificationProcessor, typeof(MediatR.NotificationProcessor<>));
            }

            if (serviceType == typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>))
            {
                yield return Of(Get15_NotificationProcessor, typeof(MediatR.NotificationProcessor<>));
            }

            if (serviceType == typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>))
            {
                yield return Of(Get16_INotificationHandler, DefaultKey.Of(0));
                yield return Of(Get17_INotificationHandler, DefaultKey.Of(1));
                yield return Of(Get7_INotificationHandler); // co-variant
            }

        }

        // typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>)
        internal static object Get0_RequestProcessor(IResolverContext r)
        {
            return new MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>(new MediatR.Examples.PingHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.Pipeline.RequestProcessor<,>), (object)null, 51, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(MediatR.IRequestHandler<MediatR.Examples.Ping, MediatR.Examples.Pong>), 44, typeof(MediatR.Examples.PingHandler), Reuse.Transient), default(object[]))), new MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>[] { new MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>(new MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>[] { new MediatR.Examples.ConstrainedRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.Pipeline.RequestProcessor<,>), (object)null, 51, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 52, FactoryType.Service, typeof(MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 53, FactoryType.Service, typeof(MediatR.Examples.ConstrainedRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.Pipeline.RequestProcessor<,>), (object)null, 51, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 52, FactoryType.Service, typeof(MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 54, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>(new MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Ping>[] { new MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Ping>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.Pipeline.RequestProcessor<,>), (object)null, 51, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 55, FactoryType.Service, typeof(MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Ping>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Ping>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 56, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Ping>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.Pipeline.RequestProcessor<,>), (object)null, 51, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(2), IfUnresolved.ReturnDefaultIfNotRegistered, 57, FactoryType.Service, typeof(MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.INotification)
        internal static object Get1_INotification(IResolverContext r)
        {
            return new MediatR.Examples.Pinged();
        }

        // typeof(MediatR.INotification)
        internal static object Get2_INotification(IResolverContext r)
        {
            return new MediatR.Examples.Ponged();
        }

        // typeof(MediatR.IRequest)
        internal static object Get3_IRequest(IResolverContext r)
        {
            return new MediatR.Examples.Jing();
        }

        // typeof(MediatR.IRequest<MediatR.Unit>)
        internal static object Get4_IRequest(IResolverContext r)
        {
            return new MediatR.Examples.Jing();
        }

        // typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>)
        internal static object Get5_INotificationHandler(IResolverContext r)
        {
            return new MediatR.Examples.PongedHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>), default(System.Type), (object)null, 41, FactoryType.Service, typeof(MediatR.Examples.PongedHandler), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(MediatR.IRequestHandler<MediatR.Examples.Ping, MediatR.Examples.Pong>)
        internal static object Get6_IRequestHandler(IResolverContext r)
        {
            return new MediatR.Examples.PingHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestHandler<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)null, 44, FactoryType.Service, typeof(MediatR.Examples.PingHandler), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(MediatR.INotificationHandler<MediatR.INotification>)
        internal static object Get7_INotificationHandler(IResolverContext r)
        {
            return new MediatR.Examples.GenericHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationHandler<MediatR.INotification>), default(System.Type), (object)null, 32, FactoryType.Service, typeof(MediatR.Examples.GenericHandler), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>)
        internal static object Get8_RequestProcessor(IResolverContext r)
        {
            return new MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>(new MediatR.Examples.JingHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), typeof(MediatR.Pipeline.RequestProcessor<>), (object)null, 58, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(MediatR.IRequestHandler<MediatR.Examples.Jing>), 37, typeof(MediatR.Examples.JingHandler), Reuse.Transient), default(object[]))), new MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>[] { new MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>(new MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>[] { new MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), typeof(MediatR.Pipeline.RequestProcessor<>), (object)null, 58, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 59, FactoryType.Service, typeof(MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 60, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>(new MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Jing>[] { new MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Jing>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), typeof(MediatR.Pipeline.RequestProcessor<>), (object)null, 58, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 61, FactoryType.Service, typeof(MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Jing>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Jing>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 62, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Jing>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), typeof(MediatR.Pipeline.RequestProcessor<>), (object)null, 58, FactoryType.Service, typeof(MediatR.Pipeline.RequestProcessor<MediatR.Examples.Jing>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(2), IfUnresolved.ReturnDefaultIfNotRegistered, 63, FactoryType.Service, typeof(MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.IMediator)
        internal static object Get9_IMediator(IResolverContext r)
        {
            return new MediatR.Mediator((MediatR.SingleInstanceFactory)r.Resolve(typeof(MediatR.SingleInstanceFactory), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IMediator), default(System.Type), (object)null, 28, FactoryType.Service, typeof(MediatR.Mediator), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(MediatR.IRequest<MediatR.Examples.Pong>)
        internal static object Get10_IRequest(IResolverContext r)
        {
            return new MediatR.Examples.Ping();
        }

        // typeof(MediatR.IRequestHandler<MediatR.Examples.Jing>)
        internal static object Get11_IRequestHandler(IResolverContext r)
        {
            return new MediatR.Examples.JingHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestHandler<MediatR.Examples.Jing>), default(System.Type), (object)null, 37, FactoryType.Service, typeof(MediatR.Examples.JingHandler), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(MediatR.IBaseRequest)
        internal static object Get12_IBaseRequest(IResolverContext r)
        {
            return new MediatR.Examples.Jing();
        }

        // typeof(MediatR.IBaseRequest)
        internal static object Get13_IBaseRequest(IResolverContext r)
        {
            return new MediatR.Examples.Ping();
        }

        // typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>)
        internal static object Get14_NotificationProcessor(IResolverContext r)
        {
            return new MediatR.NotificationProcessor<MediatR.Examples.Pinged>(new MediatR.INotificationHandler<MediatR.Examples.Pinged>[] { new MediatR.Examples.GenericHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), typeof(MediatR.NotificationProcessor<>), (object)null, 64, FactoryType.Service, typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), typeof(MediatR.INotificationHandler<MediatR.INotification>), (object)null, IfUnresolved.ReturnDefaultIfNotRegistered, 32, FactoryType.Service, typeof(MediatR.Examples.GenericHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.PingedHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), typeof(MediatR.NotificationProcessor<>), (object)null, 64, FactoryType.Service, typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 40, FactoryType.Service, typeof(MediatR.Examples.PingedHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.ConstrainedPingedHandler<MediatR.Examples.Pinged>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), typeof(MediatR.NotificationProcessor<>), (object)null, 64, FactoryType.Service, typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 65, FactoryType.Service, typeof(MediatR.Examples.ConstrainedPingedHandler<MediatR.Examples.Pinged>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.PingedAlsoHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), typeof(MediatR.NotificationProcessor<>), (object)null, 64, FactoryType.Service, typeof(MediatR.NotificationProcessor<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 43, FactoryType.Service, typeof(MediatR.Examples.PingedAlsoHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>)
        internal static object Get15_NotificationProcessor(IResolverContext r)
        {
            return new MediatR.NotificationProcessor<MediatR.Examples.Ponged>(new MediatR.INotificationHandler<MediatR.Examples.Ponged>[] { new MediatR.Examples.GenericHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>), typeof(MediatR.NotificationProcessor<>), (object)null, 66, FactoryType.Service, typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Ponged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>), typeof(MediatR.INotificationHandler<MediatR.INotification>), (object)null, IfUnresolved.ReturnDefaultIfNotRegistered, 32, FactoryType.Service, typeof(MediatR.Examples.GenericHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.PongedHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>), typeof(MediatR.NotificationProcessor<>), (object)null, 66, FactoryType.Service, typeof(MediatR.NotificationProcessor<MediatR.Examples.Ponged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Ponged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 41, FactoryType.Service, typeof(MediatR.Examples.PongedHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>)
        internal static object Get16_INotificationHandler(IResolverContext r)
        {
            return new MediatR.Examples.PingedHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(0), 40, FactoryType.Service, typeof(MediatR.Examples.PingedHandler), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>)
        internal static object Get17_INotificationHandler(IResolverContext r)
        {
            return new MediatR.Examples.PingedAlsoHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(1), 43, FactoryType.Service, typeof(MediatR.Examples.PingedAlsoHandler), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

    }
}
