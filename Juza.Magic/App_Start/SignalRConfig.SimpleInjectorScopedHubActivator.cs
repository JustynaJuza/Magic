using Microsoft.AspNet.SignalR.Hubs;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;

namespace Juza.Magic
{
    public partial class SignalRConfig
    {
        private sealed class SimpleInjectorScopedHubActivator : IHubActivator
        {
            private readonly Container _container;

            public SimpleInjectorScopedHubActivator(Container container)
            {
                _container = container;
            }

            public IHub Create(HubDescriptor descriptor)
            {
                Scope scope = null;

                try
                {
                    scope = _container.BeginExecutionContextScope();

                    return (IHub) _container.GetInstance(descriptor.HubType);
                }
                catch
                {
                    scope?.Dispose();
                    throw;
                }
            }
        }
    }
}