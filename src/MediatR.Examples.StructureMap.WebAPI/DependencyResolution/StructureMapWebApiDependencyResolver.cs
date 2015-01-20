// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructureMapWebApiDependencyResolver.cs" company="Web Advanced">
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

using System.Web.Http.Dependencies;
using StructureMap;

namespace MediatR.Examples.StructureMap.WebAPI.DependencyResolution
{
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The structure map dependency resolver.
    /// </summary>
    public class StructureMapWebApiDependencyResolver : StructureMapWebApiDependencyScope, IDependencyResolver
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public StructureMapWebApiDependencyResolver(IContainer container)
            : base(container)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The begin scope.
        /// </summary>
        /// <returns>
        /// The System.Web.Http.Dependencies.IDependencyScope.
        /// </returns>
        public IDependencyScope BeginScope()
        {
            var resolver = new StructureMapWebApiDependencyResolver(CurrentNestedContainer);

            ServiceLocatorProvider provider = () => resolver;

            CurrentNestedContainer.Configure(cfg => cfg.For<ServiceLocatorProvider>().Use(provider));
            
            return resolver;
        }

        #endregion
    }
}