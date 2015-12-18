// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MediatR.Examples.StructureMap.WebAPI.DependencyResolution {
    using global::StructureMap.Configuration.DSL;
    using global::StructureMap.Graph;

    public class DefaultRegistry : Registry {
        #region Constructors and Destructors

        public DefaultRegistry() {
            Scan(
                scan => {
                    scan.TheCallingAssembly();
                    scan.AssemblyContainingType<PingHandler>();
                    scan.WithDefaultConventions();
					scan.With(new ControllerConvention());
                    scan.AddAllTypesOf(typeof(IRequestHandler<,>));
                    scan.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                    scan.AddAllTypesOf(typeof(INotificationHandler<>));
                    scan.AddAllTypesOf(typeof(IAsyncNotificationHandler<>));
                });
            For<IMediator>().Use<Mediator>();
            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
        }

        #endregion
    }
}