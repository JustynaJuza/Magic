using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Magic.Models.DataContext;

namespace Magic.Hubs
{
    [Authorize]
    public class GameHub : ConnectionHub
    {
        private MagicDBContext context = new MagicDBContext();

        #region GROUPS
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