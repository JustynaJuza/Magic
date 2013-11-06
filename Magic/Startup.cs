using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Magic.Startup))]
namespace Magic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
