// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControllerConvention.cs" company="Web Advanced">
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
    using System;
    using System.Web.Mvc;
    using global::StructureMap.Configuration.DSL;
    using global::StructureMap.Graph;
    using global::StructureMap.Pipeline;
    using global::StructureMap.TypeRules;

    public class ControllerConvention : IRegistrationConvention {
        public void Process(Type type, Registry registry)
        {
            if (type.CanBeCastTo<Controller>() && !type.IsAbstract)
            {
                registry.For(type).LifecycleIs(new UniquePerRequestLifecycle());
            }
        }
    }
}