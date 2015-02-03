namespace MediatR.Examples.StructureMap.WebAPI.DependencyResolution {
    using System.Web;
    using global::StructureMap.Web.Pipeline;
    using App_Start;
    using Microsoft.Practices.ServiceLocation;

    public class StructureMapScopeModule : IHttpModule {
        #region Public Methods and Operators

        public void Dispose() {
        }

        public void Init(HttpApplication context) {
            context.BeginRequest += (sender, e) =>
            {
                StructuremapMvc.StructureMapDependencyScope.CreateNestedContainer();
                var serviceLocatorProvider = new ServiceLocatorProvider(() => StructuremapMvc.StructureMapDependencyScope);
                StructuremapMvc.StructureMapDependencyScope.CurrentNestedContainer.Configure(cfg => cfg.For<ServiceLocatorProvider>().Use(serviceLocatorProvider));
            };
            context.EndRequest += (sender, e) => {
                HttpContextLifecycle.DisposeAndClearAll();
                StructuremapMvc.StructureMapDependencyScope.DisposeNestedContainer();
            };
        }

        #endregion
    }
}