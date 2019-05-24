using System.Web.Mvc;
using System.Web.Routing;

namespace Dxc.Shq.WebApi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // elFinder's connector route
            routes.MapRoute(null, "connector", new { controller = "File", action = "Index" });


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
