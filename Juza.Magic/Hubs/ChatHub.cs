using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Juza.Magic.Hubs
{
    public interface IChatHub
    {
        void addMessage(string roomId, string time, string sender, string senderColor, string message, bool activateTabAfterwards = false);
        void closeChatRoom(string roomId);
        void updateChatRoomUsers(string chatUsersList, string roomId);
    }

    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private readonly IChatServiceFactory _chatServiceFactory;
        private IChatService _chatService;
        private IChatService chatService
        {
            get
            {
                if (_chatService == null)
                {
                    _chatService = _chatServiceFactory.Create(
                        Context.User.Identity.GetUserId<int>(),
                        Context.ConnectionId,
                        Clients,
                        Groups);
                }
                return _chatService;
            }
        }

        public ChatHub(IChatServiceFactory chatServiceFactory)
        {
            _chatServiceFactory = chatServiceFactory;

        }

        public override Task OnConnected()
        {
            chatService.OnConnected();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            chatService.OnDisconnected();
            return base.OnDisconnected(stopCalled);
        }
    }
}