using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Magic.Startup))]
namespace Magic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = SimpleInjectorConfig.ConfigureDependencyInjectionContainer();
            SignalRConfig.ConfigureSignalR(app, container);

            ConfigureAuth(app);
        }
    }
}
