using System;
using Microsoft.AspNet.SignalR;
using Magic.Models;

namespace Magic.Hubs
{
    public interface IChatNotificationManager
    {
        void DisplayPause(User user, string gameId);
    }

    public class ChatNotificationManager : IChatNotificationManager
    {
        private readonly IChatHub _chatHub;

        public ChatNotificationManager(IChatHub chatHub)
        {
            _chatHub = chatHub;
        }


        public void DisplayPause(User user, string gameId)
        {
            _chatHub.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, "has paused the game.");
        }
    }
}