using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Magic
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            // Initialise database.
            System.Data.Entity.Database.SetInitializer<Magic.Models.DataContext.MagicDBContext>(null);
            // using (var MagicDB = new Magic.Models.DataContext.MagicDBContext()) { }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Enable automatic migrations.
            var migrator = new System.Data.Entity.Migrations.DbMigrator(new Migrations.Configuration());
            migrator.Update();

            // Initialise dependency injection resolver.
            //Magic.App_Start.SimpleInjectorInitializer.Initialize();
        }
    }
}
