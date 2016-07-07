using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using SimpleInjector;
using System;

namespace Juza.Magic
{
    public partial class SignalRConfig
    {
        public static void AdjustConnectionTimeouts()
        {
            // Make long polling connections wait a maximum of 110 seconds for a
            // response. When that time expires, trigger a timeout command and
            // make the client reconnect.
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            // Wait a maximum of 30 seconds after a transport connection is lost
            // before raising the Disconnected event to terminate the SignalR connection.
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(6);

            // For transports other than long polling, send a keepalive packet every
            // 10 seconds. 
            // This value must be no more than 1/3 of the DisconnectTimeout value.
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(2);
        }

        public static void Configure(IAppBuilder app, Container container)
        {
            var config = new HubConfiguration
            {
                EnableDetailedErrors = true
            };

            AdjustConnectionTimeouts();

            var activator = new SimpleInjectorScopedHubActivator(container);
            GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => activator);

            app.MapSignalR(config);
        }
    }
}