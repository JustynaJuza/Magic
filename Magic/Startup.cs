using Magic.Models.Extensions;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(Magic.Startup))]
namespace Magic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            try {
                var container = SimpleInjectorConfig.ConfigureDependencyInjectionContainer();
                SignalRConfig.ConfigureSignalR(app, container);
            }
            catch(Exception ex)
            {
                ex.LogException();
            }

            ConfigureAuth(app);
        }
    }
}
