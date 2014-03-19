namespace MediatR.Examples.StructureMap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::StructureMap;
    using Microsoft.Practices.ServiceLocation;

    public class StructureMapServiceLocator : ServiceLocatorImplBase
    {
        private readonly IContainer _container;

        public StructureMapServiceLocator(IContainer container)
        {
            _container = container;
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<object>();
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return serviceType.IsAbstract || serviceType.IsInterface
                           ? _container.TryGetInstance(serviceType)
                           : _container.GetInstance(serviceType);
            }

            return _container.GetInstance(serviceType, key);
        }
    }
}