using MediatR.Examples.StructureMap.WebAPI;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace MediatR.Examples.StructureMap.WebAPI
{
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
