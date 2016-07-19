using Juza.Magic.Models.DataContext;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Juza.Magic
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Disable default DB initializer so it's not trying to recreate the DB.
            Database.SetInitializer<MagicDbContext>(null);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var deleteAllConnections = @"TRUNCATE TABLE [ChatRoomUsers];
                                         TRUNCATE TABLE [ChatRoomConnections];
                                         DELETE FROM [UserConnections];";

            using (var context = new MagicDbContext())
            {
                context.Database.ExecuteSqlCommand(deleteAllConnections);
            }
        }
    }
}
