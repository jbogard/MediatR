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
Generation is completed successfully.
--------------------------------------------------------------------------------------------------------
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
            lastFactoryID = 66; // generated: equals to last used Factory.FactoryID 
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType)
        {
            if (serviceType == typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>))
                service = Get0_INotificationMediator(this);

            else
            if (serviceType == typeof(MediatR.INotificationMediator<MediatR.Examples.Ponged>))
                service = Get1_INotificationMediator(this);

            else
            if (serviceType == typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>))
                service = Get2_IRequestMediator(this);

            else
            if (serviceType == typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>))
                service = Get3_IRequestMediator(this);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service,
            Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
        {
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveManyGenerated(ref IEnumerable<ResolveManyResult> services, Type serviceType) 
        {
            services = ResolveManyGenerated(serviceType);
        }

        [ExcludeFromCodeCoverage]
        private IEnumerable<ResolveManyResult> ResolveManyGenerated(Type serviceType)
        {
            if (serviceType == typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>))
            {
                yield return Of(Get0_INotificationMediator, typeof(MediatR.INotificationMediator<>));
            }

            if (serviceType == typeof(MediatR.INotificationMediator<MediatR.Examples.Ponged>))
            {
                yield return Of(Get1_INotificationMediator, typeof(MediatR.INotificationMediator<>));
            }

            if (serviceType == typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>))
            {
                yield return Of(Get2_IRequestMediator, typeof(MediatR.IRequestMediator<,>));
            }

            if (serviceType == typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>))
            {
                yield return Of(Get3_IRequestMediator, typeof(MediatR.IRequestMediator<,>));
            }

        }

        // typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>)
        internal static object Get0_INotificationMediator(IResolverContext r)
        {
            return new MediatR.NotificationMediator<MediatR.Examples.Pinged>(new MediatR.INotificationHandler<MediatR.Examples.Pinged>[] { new MediatR.Examples.GenericHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>), typeof(MediatR.INotificationMediator<>), (object)null, 50, FactoryType.Service, typeof(MediatR.NotificationMediator<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), typeof(MediatR.INotificationHandler<MediatR.INotification>), (object)null, IfUnresolved.ReturnDefaultIfNotRegistered, 34, FactoryType.Service, typeof(MediatR.Examples.GenericHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.PingedHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>), typeof(MediatR.INotificationMediator<>), (object)null, 50, FactoryType.Service, typeof(MediatR.NotificationMediator<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 42, FactoryType.Service, typeof(MediatR.Examples.PingedHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.ConstrainedPingedHandler<MediatR.Examples.Pinged>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>), typeof(MediatR.INotificationMediator<>), (object)null, 50, FactoryType.Service, typeof(MediatR.NotificationMediator<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 51, FactoryType.Service, typeof(MediatR.Examples.ConstrainedPingedHandler<MediatR.Examples.Pinged>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.PingedAlsoHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationMediator<MediatR.Examples.Pinged>), typeof(MediatR.INotificationMediator<>), (object)null, 50, FactoryType.Service, typeof(MediatR.NotificationMediator<MediatR.Examples.Pinged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Pinged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Pinged>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 45, FactoryType.Service, typeof(MediatR.Examples.PingedAlsoHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.INotificationMediator<MediatR.Examples.Ponged>)
        internal static object Get1_INotificationMediator(IResolverContext r)
        {
            return new MediatR.NotificationMediator<MediatR.Examples.Ponged>(new MediatR.INotificationHandler<MediatR.Examples.Ponged>[] { new MediatR.Examples.GenericHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationMediator<MediatR.Examples.Ponged>), typeof(MediatR.INotificationMediator<>), (object)null, 52, FactoryType.Service, typeof(MediatR.NotificationMediator<MediatR.Examples.Ponged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Ponged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>), typeof(MediatR.INotificationHandler<MediatR.INotification>), (object)null, IfUnresolved.ReturnDefaultIfNotRegistered, 34, FactoryType.Service, typeof(MediatR.Examples.GenericHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.PongedHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.INotificationMediator<MediatR.Examples.Ponged>), typeof(MediatR.INotificationMediator<>), (object)null, 52, FactoryType.Service, typeof(MediatR.NotificationMediator<MediatR.Examples.Ponged>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.INotificationHandler<MediatR.Examples.Ponged>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.INotificationHandler<MediatR.Examples.Ponged>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 43, FactoryType.Service, typeof(MediatR.Examples.PongedHandler), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>)
        internal static object Get2_IRequestMediator(IResolverContext r)
        {
            return new MediatR.RequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>(new MediatR.Examples.PingHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.IRequestMediator<,>), (object)null, 53, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(MediatR.IRequestHandler<MediatR.Examples.Ping, MediatR.Examples.Pong>), 46, typeof(MediatR.Examples.PingHandler), Reuse.Transient), default(object[]))), new MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>[] { new MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>(new MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>[] { new MediatR.Examples.ConstrainedRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.IRequestMediator<,>), (object)null, 53, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 54, FactoryType.Service, typeof(MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 55, FactoryType.Service, typeof(MediatR.Examples.ConstrainedRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))), new MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.IRequestMediator<,>), (object)null, 53, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 54, FactoryType.Service, typeof(MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 56, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>(new MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Ping>[] { new MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Ping>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.IRequestMediator<,>), (object)null, 53, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 57, FactoryType.Service, typeof(MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Ping>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Ping>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 58, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Ping>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), typeof(MediatR.IRequestMediator<,>), (object)null, 53, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), default(System.Type), (object)DefaultKey.Of(2), IfUnresolved.ReturnDefaultIfNotRegistered, 59, FactoryType.Service, typeof(MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Ping, MediatR.Examples.Pong>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

        // typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>)
        internal static object Get3_IRequestMediator(IResolverContext r)
        {
            return new MediatR.RequestMediator<MediatR.Examples.Jing, MediatR.Unit>(new MediatR.Examples.JingHandler((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>), typeof(MediatR.IRequestMediator<,>), (object)null, 60, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(MediatR.IRequestHandler<MediatR.Examples.Jing, MediatR.Unit>), 39, typeof(MediatR.Examples.JingHandler), Reuse.Transient), default(object[]))), new MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>[] { new MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>(new MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>[] { new MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>), typeof(MediatR.IRequestMediator<,>), (object)null, 60, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 61, FactoryType.Service, typeof(MediatR.Pipeline.RequestPostProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 62, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPostProcessor<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>(new MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Jing>[] { new MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Jing>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>), typeof(MediatR.IRequestMediator<,>), (object)null, 60, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(1), IfUnresolved.ReturnDefaultIfNotRegistered, 63, FactoryType.Service, typeof(MediatR.Pipeline.RequestPreProcessorBehavior<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Jing>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.Pipeline.IRequestPreProcessor<MediatR.Examples.Jing>), default(System.Type), (object)DefaultKey.Of(0), IfUnresolved.ReturnDefaultIfNotRegistered, 64, FactoryType.Service, typeof(MediatR.Examples.GenericRequestPreProcessor<MediatR.Examples.Jing>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) }), new MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>((System.IO.TextWriter)r.Resolve(typeof(System.IO.TextWriter), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(MediatR.IRequestMediator<MediatR.Examples.Jing, MediatR.Unit>), typeof(MediatR.IRequestMediator<,>), (object)null, 60, FactoryType.Service, typeof(MediatR.RequestMediator<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, RequestFlags.IsResolutionCall).Push(typeof(System.Collections.Generic.IEnumerable<MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>>), default(System.Type), (object)null, 2, FactoryType.Wrapper, default(System.Type), Reuse.Transient, ((RequestFlags)0)).Push(typeof(MediatR.IPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), default(System.Type), (object)DefaultKey.Of(2), IfUnresolved.ReturnDefaultIfNotRegistered, 65, FactoryType.Service, typeof(MediatR.Examples.GenericPipelineBehavior<MediatR.Examples.Jing, MediatR.Unit>), Reuse.Transient, ((RequestFlags)0), 0), default(object[]))) });
        }

    }
}
