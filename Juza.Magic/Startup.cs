using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Juza.Magic.Startup))]
namespace Juza.Magic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = SimpleInjectorInitializer.Initialize(app);
            SignalRConfig.ConfigureSignalR(app, container);
            ConfigureAuth(app, container);
        }
    }
}
