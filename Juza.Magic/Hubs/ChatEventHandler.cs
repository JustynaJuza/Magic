using Juza.Magic.Helpers;
using Juza.Magic.Models.Entities.Chat;
using Microsoft.AspNet.SignalR.Hubs;
using System;

namespace Juza.Magic.Hubs
{
    public class JoinedRoomEventArgs : EventArgs
    {
        public int UserId { get; set; }
        public string ConnectionId { get; set; }
        public ChatRoom ChatRoom { get; set; }
        public IHubConnectionContext<IChatHub> Clients { get; set; }
    }

    public delegate void JoinedRoomEventHandler(object sender, JoinedRoomEventArgs e);


    public interface IChatEventHandler
    {
        void JoinedRoomEventHandler(object sender, JoinedRoomEventArgs e);
    }

    public class ChatEventHandler : IChatEventHandler
    {
        private readonly IViewRenderer _viewRenderer;

        public ChatEventHandler(IViewRenderer viewRenderer)
        {
            _viewRenderer = viewRenderer;
        }

        public void JoinedRoomEventHandler(object sender, JoinedRoomEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Joined room" + e.ConnectionId + " " + e.UserId + " " + e.ChatRoom.Id);
            //var viewModel = e.ChatRoom.ToViewModel<ChatRoom, ChatRoomViewModel>();
            //var chatRoomPartial = _viewRenderer.RenderPartialViewToString("_ChatRoomPartial", viewModel);

            //e.Clients.Client(e.ConnectionId).appendChatRoomPartial(chatRoomPartial);
        }
    }
}
