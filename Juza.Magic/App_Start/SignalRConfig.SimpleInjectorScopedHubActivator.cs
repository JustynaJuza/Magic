using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using System.Threading.Tasks;

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

            private sealed class ScopedHubDecorator : IHub
            {
                private readonly IHub _decoratee;
                private readonly Scope _scope;

                public ScopedHubDecorator(IHub decoratee, Scope scope)
                {
                    _decoratee = decoratee;
                    _scope = scope;
                }

                public IGroupManager Groups
                {
                    get { return _decoratee.Groups; }
                    set { _decoratee.Groups = value; }
                }

                public Task OnDisconnected(bool stopCalled)
                {
                    return _decoratee.OnDisconnected(stopCalled);
                }

                public HubCallerContext Context
                {
                    get { return _decoratee.Context; }
                    set { _decoratee.Context = value; }
                }

                IHubCallerConnectionContext<dynamic> IHub.Clients
                {
                    get { return _decoratee.Clients; }
                    set { _decoratee.Clients = value; }
                }


                public Task OnConnected()
                {
                    return _decoratee.OnConnected();
                }

                public Task OnReconnected()
                {
                    return _decoratee.OnReconnected();
                }

                public void Dispose()
                {
                    try
                    {
                        _decoratee.Dispose();
                    }
                    finally
                    {
                        _scope.Dispose();
                    }
                }
            }
        }
    }
}