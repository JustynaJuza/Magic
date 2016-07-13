using Juza.Magic.Hubs;
using Juza.Magic.Models;
using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Projections;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Juza.Magic
{

    // Resolving default MVC 5 template for ASP.Net Identity 2.0
    // from http://simpleinjector.codeplex.com/discussions/564822
    public static class SimpleInjectorInitializer
    {
        private static ScopedLifestyle _hybridLifestyle;

        public static Container Initialize(IAppBuilder app)
        {
            var container = new Container();

            SetDefaultHybridLifestyle(container);
            InitializeContainer(container, app);

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            return container;
        }

        public static void SetDefaultHybridLifestyle(Container container)
        {
            _hybridLifestyle = Lifestyle.CreateHybrid(
                () => container.GetCurrentExecutionContextScope() != null,
                new ExecutionContextScopeLifestyle(),
                new WebRequestLifestyle());

            container.Options.DefaultScopedLifestyle = _hybridLifestyle;
        }

        public static void InitializeContainer(Container container, IAppBuilder app)
        {
            container.RegisterSingleton(app);

            // Data Access
            container.Register<MagicDbContext>(_hybridLifestyle);
            container.Register<IDbContext>(container.GetInstance<MagicDbContext>, _hybridLifestyle);

            RegisterMappings(container);

            // Hubs
            RegisterHubs(container);

            //container.Register<IPathProvider, PathProvider>();
            //container.Register<IFileHandler, FileHandler>();
            container.Register<ICardService, CardService>();

            // ASP.Net Identity
            container.RegisterPerWebRequest<IRoleStore<Role, int>>(() => new RoleStore<Role, int, UserRole>(container.GetInstance<MagicDbContext>()));
            container.RegisterPerWebRequest<RoleManager<Role, int>>();

            container.RegisterPerWebRequest<IUserStore<User, int>>(() => new UserStore<User, Role, int, UserLogin, UserRole, UserClaim>(container.GetInstance<MagicDbContext>()));
            container.RegisterPerWebRequest<ApplicationUserManager>();
            container.RegisterInitializer<ApplicationUserManager>(manager => ApplicationUserManager.Configure(manager, app));
            container.RegisterPerWebRequest<ApplicationSignInManager, ApplicationSignInManager>();

            container.RegisterPerWebRequest(() => container.IsVerifying()
                ? new OwinContext(new Dictionary<string, object>()).Authentication
                : HttpContext.Current.GetOwinContext().Authentication);

            // MVC Controllers
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();

            //// Lists
            //container.Register(typeof(IListModelFilterProvider<,>), new[] { Assembly.GetCallingAssembly() });
            //container.RegisterConditional(typeof(IListModelFilterProvider<,>), typeof(NullListModelFilterProvider<,>), x => !x.Handled);
            //container.AppendToCollection(typeof(IListModelFilterTextProvider<>), typeof(EntityListModelFilterTextProvider<>));
            //container.AppendToCollection(typeof(IListModelFilterTextProvider<>), typeof(PlainListModelFilterTextProvider<>));
            //container.RegisterCollection(typeof(IListModelFilterTextProvider<>), Assembly.GetCallingAssembly());
            //container.Register(typeof(IListModelFilterTextProvider<>), typeof(CompositeListModelFilterTextProvider<>));
            //container.Register(typeof(IListViewModelFactory<,,>), typeof(ListViewModelFactory<,,>));
            //container.Register(typeof(IPagedListFactory<,,>), typeof(PagedListFactory<,,>));

            //// Modifications
            //container.Register(typeof(IModifier<>), new[] { Assembly.GetCallingAssembly() });
            //container.Register(typeof(IModifyViewModelFactory<,,>), new[] { Assembly.GetCallingAssembly() });
            //container.Register<IImageService, ImageService>();
            //container.Register<IRatingSystemsService, RatingSystemsService>();

            //// Authentication & Authorization
            //container.RegisterDecorator(typeof(IListViewModelFactory<,,>), typeof(AuthorizationInterceptionListViewModelFactoryDecorator<,,>));
            //container.Register(typeof(IAuthorizationInterceptor<>), new[] { Assembly.GetCallingAssembly() });
            //container.Register(typeof(IAuthorizationInterceptor<>), typeof(CorporateClientListAuthorizationInterceptor<>));
            //container.RegisterConditional(typeof(IAuthorizationInterceptor<>), typeof(NullAuthorizationInterceptor<>), x => !x.Handled);
            //container.Register<IAppUserProvider, AppUserProvider>();
        }

        private static void RegisterHubs(Container container)
        {
            container.Register<IChatDataProvider, ChatDataProvider>(_hybridLifestyle);
            container.Register<IChatServiceFactory, ChatServiceFactory>(_hybridLifestyle);
            container.Register<ChatHub, ChatHub>(_hybridLifestyle);

            //container.Register<GameHub, GameHub>(HybridLifestyle);

            //container.Register(() => GlobalHost.ConnectionManager.GetHubContext<GameHub, IGameHub>(), Lifestyle.Singleton);
            //container.Register(() => GlobalHost.ConnectionManager.GetHubContext<ChatHub, IChatHub>(), Lifestyle.Singleton);
            //var hubs = Assembly.GetExecutingAssembly().GetExportedTypes().Where(x => x.IsAssignableFrom(typeof(Hub)));
        }

        private static void RegisterMappings(Container container)
        {
            //var mappingTypes = new[] { typeof(IMapping<,>), typeof(IQueryMapping<,>) };
            //var mappingMethods = typeof(MappingFactory).GetMethods(BindingFlags.Static | BindingFlags.Public);

            //var factoryMethods = mappingMethods
            //    .Where(method => method.ReturnType.IsGenericType
            //        && mappingTypes.Contains(method.ReturnType.GetGenericTypeDefinition()))
            //    .Select(method => new
            //    {
            //        ServiceTypes = method.ReturnType.GetInterfaces().Concat(new[] { method.ReturnType }),
            //        Method = new Func<object>(() => method.Invoke(null, null))
            //    });

            //foreach (var factoryMethod in factoryMethods)
            //{
            //    foreach (var serviceType in factoryMethod.ServiceTypes)
            //    {
            //        container.Register(serviceType, factoryMethod.Method);
            //    }
            //}

            var assembliesWithMappings = new[] { typeof(MappingFactory).Assembly };
            container.Register(typeof(IObjectMapping<,>), assembliesWithMappings);
            //container.Register(typeof(IMapping<,>), assembliesWithMappings);

            // Let AutoMapper act as the default mapper
            container.RegisterConditional(typeof(IObjectMapping<,>), typeof(DefaultAutoMapping<,>), x => !x.Handled);
            //container.RegisterConditional(typeof(IMapping<,>), typeof(DefaultAutoMapping<,>), x => !x.Handled);
        }

    }
}