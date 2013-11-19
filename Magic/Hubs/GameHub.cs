using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace Magic.Hubs
{
    [Authorize]
    public class GameHub : ConnectionHub
    {
        #region GROUPS
        public async Task JoinRoom(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " joined.");
        }

        public async Task LeaveRoom(string roomName)
        {
            try
            {
                await Groups.Remove(Context.ConnectionId, roomName);
                Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " left.");
            }
            catch (Exception) { }
        }
        #endregion GROUPS
    }
}