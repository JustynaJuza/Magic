using Magic.Helpers;
using Magic.Hubs;
using Magic.Models.DataContext;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using System.Reflection;
using System.Web.Mvc;

namespace Magic
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

        public static Container ConfigureDependencyInjectionContainer()
        {
            var container = new Container();

            //container.Register<IUserStore<ApplicationUser>>(() => new UserStore<ApplicationUser>(new ApplicationDbContext()));
            //container.Register<ApplicationUserManager, ApplicationUserManager>();
            
            // Data Access
            container.Register<MagicDbContext>(new WebRequestLifestyle());
            container.Register<IPathProvider, PathProvider>();
            container.Register<IFileHandler, FileHandler>();

            // Hubs
            container.Register(() => new ChatHub(new MagicDbContext()));
            container.Register(() => new AdminHub(new FileHandler(new PathProvider())));

            // MVC
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            return container;
        }
    }
}