using System;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Extensions;

namespace MediatR.Examples.SimpleInjector
{
    internal static class SimpleInjectorExtensions
    {
        /// <summary>
        /// Registers all the concrete, non-generic, public and internal types that are located 
        /// in the given set of assemblies that implement the given <paramref name="openGenericServiceType"/> 
        /// using <see cref="M:OpenGenericBatchRegistrationExtensions.GetTypesToRegister"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        public static void BatchRegisterOpenGeneric(this Container container, Type openGenericServiceType, params Assembly[] assemblies)
        {
            var types = OpenGenericBatchRegistrationExtensions
                .GetTypesToRegister(
                    container,
                    openGenericServiceType,
                    AccessibilityOption.AllTypes,
                    assemblies)
                .ToList();

            container.RegisterAll(openGenericServiceType, types);
        }
    }
}
