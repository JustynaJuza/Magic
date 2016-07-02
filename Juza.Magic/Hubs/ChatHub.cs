using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Juza.Magic.Hubs
{
    public interface IChatHub
    {
        void addMessage(string roomId, string time, string sender, string senderColor, string message, bool activateTabAfterwards = false);
        void closeChatRoom(string roomId);
        void updateChatRoomUsers(string chatUsersList, string roomId);
    }

    //[Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private readonly IDbContext _context;
        private readonly IChatDataProvider _chatUserProvider;

        public ChatHub(IDbContext context,
            IChatDataProvider chatUserProvider)
        {
            _context = context;
            _chatUserProvider = chatUserProvider;
        }


        public void UpdateChatRoomUsers(string roomId)
        {
            var chatRoomUsers = _chatUserProvider.GetChatRoomUsers(roomId);
            Clients.Group(roomId).updateChatRoomUsers(Json.Encode(chatRoomUsers), roomId);

        }

        public void Send(string messageText, string roomId)
        {
            var sender = _chatUserProvider.GetUser(Context.User.Identity.GetUserId<int>());
            var timeSent = DateTime.Now;
            Clients.Group(roomId).addMessage(roomId, timeSent.ToString("HH:mm:ss"), sender.UserName, sender.ColorCode, messageText);

            _chatUserProvider.SaveMessage(Context.User.Identity.GetUserId<int>(), roomId, messageText, timeSent);
        }

        public void UserStatusUpdate(int userId, UserStatus status, string roomId)
        {
            var user = _chatUserProvider.GetUser(userId);
            _chatUserProvider.UserStatusUpdate(userId, status);

            Clients.Group(roomId)
                .addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, UserStatusBroadcastMessage(status));
        }

        private string UserStatusBroadcastMessage(UserStatus status)
        {
            switch (status)
            {
                case UserStatus.AFK: return " is away.";
                case UserStatus.Online: return " joined the conversation.";
                case UserStatus.Offline: return " left.";
                case UserStatus.Observing: return " is observing a duel.";
                case UserStatus.Playing: return " concentrates on a game right now.";
                //case UserStatus.Ready: return " is ready for action.";
                //case UserStatus.Unready: return " seems to be not prepared!";
                default: return null;
            }
        }

        //private bool ValidateMessage(string messageText, string recipientName, out User recipient)
        //{
        //    recipient = null;

        //    if (messageText.Length <= 0) return false; // No message text at all.
        //    if (recipientName.Length <= 0) return true; // Group chat message.

        //    using (var _context = new MagicDbContext())
        //    {
        //        // Look for recipient.
        //        recipient = _context.Users.FirstOrDefault(u => u.UserName == recipientName);
        //        if (recipient == null)
        //        {
        //            // Recipient included but invalid, alert sender.
        //            Clients.Caller.addMessage(ChatRoom.DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo",
        //                "#000000", "- no such user found, have you misspelled the name?"); //, recipientName, "#696969");
        //            return false;
        //        }

        //        // Valid recipient found.
        //        if (recipient.Status == UserStatus.Offline)
        //        {
        //            // Valid recipient but is offline, alert sender.
        //            Clients.Caller.addMessage(ChatRoom.DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo",
        //                "#000000", "is currently offline and unable to receive messages."); //, recipient.UserName, recipient.ColorCode);
        //            return false;
        //        }

        //        return true; // Valid message for recipient.
        //    }
        //}

        public async void SubscribeChatRoom(int userId, string roomId)
        {
            var addToGroup = Groups.Add(Context.ConnectionId, roomId);
            _chatUserProvider.SubscribeChatRoom(roomId, Context.ConnectionId, userId);

            if (roomId != ChatRoom.DefaultRoomId)
            {
                UpdateChatRoomUsers(roomId);
            }

            await addToGroup;
        }

        public Task UnsubscribeChatRoom(string roomId)
        {
            var connections = _chatUserProvider.UnsubscribeChatRoom(Context.User.Identity.GetUserId<int>(), roomId);

            Clients.Clients(connections).closeChatRoom(roomId);

            var unsubscribeUser = connections.Select(connectionId => Groups.Remove(connectionId, roomId));

            return Task.WhenAll(unsubscribeUser);
        }

        public Task SubscribeActiveChatRooms(int userId, string connectionId)
        {
            var activeChatRooms = _chatUserProvider.SubscribeActiveChatRooms(userId, connectionId);

            var subscribeRooms = activeChatRooms.Select(roomId => Groups.Add(connectionId, roomId));
            return Task.WhenAll(subscribeRooms);
        }

        public Task SubscribeActiveConnections(IEnumerable<string> connections, string roomId)
        {
            var subscribeUser = connections.Select(connectionId => Groups.Add(connectionId, roomId));

            return Task.WhenAll(subscribeUser);
        }

        public async Task SubscribeGameChat(string roomId)
        {
            var activeUnsubscribedConnections = _chatUserProvider.SubscribeGameChat(Context.User.Identity.GetUserId<int>(), Context.ConnectionId, roomId).ToList();

            // Send info message on joining group - also opens the chat tab for joining user.
            if (activeUnsubscribedConnections.Any())
            {
                var subscribe = SubscribeActiveConnections(activeUnsubscribedConnections, roomId);

                var chatRoom = _chatUserProvider.GetChatRoom(roomId);
                Clients.Caller.addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), chatRoom.Name, chatRoom.TabColorCode, " Welcome back!", true);

                await subscribe;
            }
            else
            {
                var user = _chatUserProvider.GetUser(Context.User.Identity.GetUserId<int>());
                Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, " entered the game.", true);
            }
        }

        public void UnsubscribeGameChat(string roomId)
        {
            _chatUserProvider.UnsubscribeGameChat(Context.User.Identity.GetUserId<int>(), Context.ConnectionId, roomId);
            //TODO
            //_gameConnectionManager.LeaveGame(connection);

            UnsubscribeChatRoom(roomId);
            System.Diagnostics.Debug.WriteLine("Unsubscribing " + roomId);

            // Sent info message on leaving group.
            var user = _chatUserProvider.GetUser(Context.User.Identity.GetUserId<int>());
            Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, (" left the game."), true);

        }

        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId<int>();
            _chatUserProvider.AddUserConnection(userId, Context.ConnectionId);

            SubscribeChatRoom(userId, ChatRoom.DefaultRoomId);
            //UpdateChatRoomUsers(ChatRoom.DefaultRoomId);

            foreach (var chatRoom in _chatUserProvider.GetChatRoomsWithUser(userId))
            {
                UpdateChatRoomUsers(chatRoom.Id);
            }

            var connectionCount = _chatUserProvider.GetUserConnectionCount(userId);
            if (connectionCount == 1)
            {
                UserStatusUpdate(userId, UserStatus.Online, ChatRoom.DefaultRoomId);
            }

            SubscribeActiveChatRooms(userId, Context.ConnectionId);

            // TODO: What when: user disconnects, other user has private chat tab open, user connects again - chat tab is unsubscribed, typing message does not notify receiving user?
            // Solution: use message notifications for that - partially implemented.

            _chatUserProvider.SaveAll();
            System.Diagnostics.Debug.WriteLine("Connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = Context.User.Identity.GetUserId<int>();
            UserStatusUpdate(userId, UserStatus.Offline, ChatRoom.DefaultRoomId);
            var chatRooms = _chatUserProvider.GetUserChatRooms(Context.User.Identity.GetUserId<int>());

            foreach (var chatRoom in chatRooms)
            {
                UpdateChatRoomUsers(chatRoom.Id);
            }

            _chatUserProvider.DeleteConnection(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        #region CHATLOG HANDLING
        // This function is called by schedule from Global.asax and uses the static _context to save recent chat messages.
        //public static bool SaveChatLogToDatabase(ChatLog currentLog)
        //{
        //    ChatLog todayLog = _context.ChatLogs.Find(currentLog.DateCreated);//_context.Set<ChatLog>().AsNoTracking().FirstOrDefault(c => c.DateCreated == currentLog.DateCreated);
        //    if (todayLog != null)
        //    {
        //        todayLog.AppendMessages(currentLog.Messages);
        //        return _context.InsertOrUpdate(todayLog, true);
        //    }

        //    // Create new log for Today.
        //    todayLog = new ChatLog
        //    {
        //        Messages = currentLog.Messages
        //    };
        //    return _context.Insert(todayLog);
        //}
        #endregion CHATLOG HANDLING

        public string GetExistingChatRoomIdForUsers(string[] recipientNames)
        {
            return _chatUserProvider.GetExistingChatRoomIdForUsers(recipientNames);
        }

        //public void CreateChatRoom(string roomId = null, bool isGameRoom = false, bool isPrivate = false, IList<string> recipientNames = null)
        //{
        //    _chatUserProvider.CreateChatRoom(roomId, isGameRoom, isPrivate, recipientNames);

        //    if (!isPrivate) return;

        //    // TODO: check how recipients behave after chacking chatroom existance and if thee can be any null exception
        //    var recipients = recipientNames.Distinct().Select(userName => _context.Query<User>().FirstOrDefault(u => u.UserName == userName)).ToList();

        //    foreach (var user in recipients)
        //    {
        //        AddUserToRoom(chatRoom.Id, user.Id);
        //        SubscribeActiveConnections(chatRoom.Id, user.Id);
        //    }
        //}
    }
}