using Magic.Helpers;
using Magic.Models.DataContext;
using RazorEngine.Templating;
using SimpleInjector;
using SimpleInjector.Extensions;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Penna.Messaging.Web
{
    public class SimpleInjectorConfig
    {
        /// <summary>
        /// A container lifestyle scoped to the current request, determined by the presence
        /// of an OperationContext (WCF) or not (Web)
        /// </summary>
        //private static Lifestyle RequestScopedLifestyle
        //{
        //    get
        //    {
        //        return Lifestyle.CreateHybrid(
        //            lifestyleSelector: () => OperationContext.Current != null,
        //            trueLifestyle: new WcfOperationLifestyle(true),
        //            falseLifestyle: new WebRequestLifestyle(true));
        //    }
        //}

        public static void RegisterContainer()
        {
            var container = new Container();

            //container.Register<IUserStore<ApplicationUser>>(() => new UserStore<ApplicationUser>(new ApplicationDbContext()));
            //container.Register<ApplicationUserManager, ApplicationUserManager>();

            // Data Access
            container.Register<MagicDbContext>(new WebRequestLifestyle());
            container.Register<IPathProvider, PathProvider>();
            container.Register<IFileHandler, FileHandler>();

            // MVC
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
    }
}