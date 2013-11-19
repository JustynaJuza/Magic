using Magic.Models;
using Magic.Models.DataContext;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Magic.Hubs
{
    public class ConnectionHub : Hub
    {
        private MagicDBContext context = new MagicDBContext();

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            using (MagicDBContext tempContext = new MagicDBContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var foundUser = tempContext.Users.Find(userId);

                var connection = foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
                if (connection == null)
                {
                    foundUser.Connections.Add(new ApplicationUserConnection
                    {
                        Id = Context.ConnectionId,
                        UserAgent = Context.Request.Headers["User-Agent"],
                        // Connected = true
                    });
                    foundUser.Status = UserStatus.Online;
                }
                else
                {
                    connection.Connected = true;
                    foundUser.Status = UserStatus.Online;
                }

                tempContext.Update(foundUser);
                ChatHub.UserActionBroadcast(userId);
            }
            System.Diagnostics.Debug.WriteLine("Connect");
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            System.Diagnostics.Debug.WriteLine("Reconnect");
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            using (MagicDBContext tempContext = new MagicDBContext())
            {
                var connection = tempContext.UserConnections.Find(Context.ConnectionId);
                connection.User.Status = UserStatus.Offline;
                ChatHub.UserActionBroadcast(connection.User.Id, false);

                tempContext.Update(connection, true);
                tempContext.Delete(connection, true);
            }
            System.Diagnostics.Debug.WriteLine("Disconnect");
            return base.OnDisconnected();
        }
        #endregion CONNECTION STATUS UPDATE
    }
}