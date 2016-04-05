using System;
using Magic.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Magic.Hubs
{
    public interface IChatNotificationManager
    {
        void DisplayPause(User user, string gameId);
    }

    public class ChatNotificationManager : IChatNotificationManager
    {
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatNotificationManager(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
            _chatHubContext = chatHubContext;
        }


        public void DisplayPause(User user, string gameId)
        {
            _chatHub.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, "has paused the game.");
        }
    }
}