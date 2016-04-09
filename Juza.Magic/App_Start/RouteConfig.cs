using System.Web.Mvc;
using System.Web.Routing;

namespace Penna.Assessment
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

#if DEBUG
            routes.IgnoreRoute("{*browserlink}", new { browserlink = @".*__browserLink.*" });
#endif

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
