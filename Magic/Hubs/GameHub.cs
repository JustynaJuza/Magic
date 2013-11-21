using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Magic.Models.DataContext;
using Magic.Models;

namespace Magic.Hubs
{
    [Authorize]
    public class GameHub : ConnectionHub
    {
        private MagicDBContext context = new MagicDBContext();

        #region GROUPS
        public static void ActivateGame(string userId, string roomName, bool joinedGame = true)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.GameHub>();
            using (MagicDBContext context = new MagicDBContext())
            {
                var foundUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);
                if (joinedGame)
                {
                    foreach (var connection in foundUser.Connections)
                    {
                        hubContext.Groups.Add(connection.Id, roomName);
                    }
                }
                else
                {
                    foreach (var connection in foundUser.Connections)
                    {
                        hubContext.Groups.Remove(connection.Id, roomName);
                    }
                }
            }
        }

        public async Task JoinGame(string game)
        {
            await Groups.Add(Context.ConnectionId, game);
        }

        public async Task LeaveGame(string game)
        {
            await Groups.Remove(Context.ConnectionId, game);
        }
        #endregion GROUPS

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            //JoinGame();
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            //LeaveGame();
            return base.OnDisconnected();
        }
        #endregion CONNECTION STATUS UPDATE
    }
}