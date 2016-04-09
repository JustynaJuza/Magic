using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Owin;
using SimpleInjector;

namespace Magic
{
    public class SignalRConfig
    {
        private class SignalRDependencyResolver : DefaultDependencyResolver
        {
            private readonly Container _container;

            public SignalRDependencyResolver(Container container)
            {
                _container = container;
            }

            public override object GetService(Type serviceType)
            {
                try
                {
                    return _container.GetInstance(serviceType);
                }
                catch (ActivationException)
                {
                    return base.GetService(serviceType);
                }
            }

            public override IEnumerable<object> GetServices(Type serviceType)
            {
                return _container.GetAllInstances(serviceType).Concat(base.GetServices(serviceType));
            }
        }

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

        public static void ConfigureSignalR(IAppBuilder app)
        {
            var config = new HubConfiguration
            {
                EnableDetailedErrors = true
            };
            AdjustConnectionTimeouts();
            app.MapSignalR(config);
        }

        public static void ConfigureSignalRDependencyResolver(Container container)
        {
            GlobalHost.DependencyResolver = new SignalRDependencyResolver(container);
        }
    }
}