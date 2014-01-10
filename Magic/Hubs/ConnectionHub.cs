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

                foundUser.Status = UserStatus.Online;
                foundUser.Connections.Add(new ApplicationUserConnection(){
                    Id = Context.ConnectionId,
                    User = foundUser
                });
                tempContext.Update(foundUser);

                if (foundUser.Connections.Count == 1)
                {
                    ChatHub.UserActionBroadcast(userId);
                }
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
                if (connection.User.Connections.Count == 1)
                {
                    ChatHub.UserActionBroadcast(connection.User.Id, false);
                }

                tempContext.Update(connection, true);
                tempContext.Delete(connection, true);
            }
            System.Diagnostics.Debug.WriteLine("Disconnect");
            return base.OnDisconnected();
        }

        #endregion CONNECTION STATUS UPDATE

        #region DISPOSE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion DISPOSE
    }
}