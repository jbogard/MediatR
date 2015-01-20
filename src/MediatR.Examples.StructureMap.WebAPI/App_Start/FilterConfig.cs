using System.Web;
using System.Web.Mvc;

namespace MediatR.Examples.StructureMap.WebAPI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
