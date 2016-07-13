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
        }

        public void Application_End()
        {
            using (var context = new MagicDbContext())
            {
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE [ChatRoomUsers];");
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE [ChatRoomConnections];");
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE [UserConnections];");
            }
        }
    }
}
