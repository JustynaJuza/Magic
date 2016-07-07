using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using WebGrease.Css.Extensions;

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
        private readonly IChatDataProvider _chatUserProvider;
        private readonly IChatService _chatService;

        private int? _userId;
        public int UserId
        {
            get
            {
                if (!_userId.HasValue)
                {
                    _userId = Context.User.Identity.GetUserId<int>();
                }
                return _userId.Value;
            }
        }

        public ChatHub(IChatDataProvider chatUserProvider,
            IChatService chatService)
        {
            _chatUserProvider = chatUserProvider;
            _chatService = chatService;
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

        public async void SubscribeDefaultChatRoom(int userId)
        {
            await SubscribeChatRoom(userId, ChatRoom.DefaultRoomId);

            var connectionCount = _chatUserProvider.GetUserConnectionCount(userId);
            if (connectionCount == 1)
            {
                UserStatusUpdate(userId, UserStatus.Online, ChatRoom.DefaultRoomId);
            }
        }

        public Task SubscribeChatRoom(int userId, string roomId)
        {
            var addToGroup = Groups.Add(Context.ConnectionId, roomId);
            _chatUserProvider.SubscribeChatRoom(roomId, Context.ConnectionId, userId);

            if (roomId != ChatRoom.DefaultRoomId)
            {
                UpdateChatRoomUsers(roomId);
            }

            return addToGroup;
        }

        public Task UnsubscribeChatRoom(string roomId)
        {
            var connections = _chatUserProvider.UnsubscribeChatRoom(Context.User.Identity.GetUserId<int>(), roomId);

            Clients.Clients(connections).closeChatRoom(roomId);

            var unsubscribeUser = connections.Select(connectionId => Groups.Remove(connectionId, roomId));

            return Task.WhenAll(unsubscribeUser);
        }
        //public Task SubscribeActiveChatRooms(int userId, string connectionId)
        //{
        //    var activeChatRooms = _chatUserProvider.SubscribeUsersChatRooms(userId, connectionId);

        //    var subscribeRooms = activeChatRooms.Select(roomId => Groups.Add(connectionId, roomId));
        //    return Task.WhenAll(subscribeRooms);
        //}

        public Task SubscribeConnectionToChatRooms(int userId, string connectionId, IList<string> chatRooms)
        {
            var subscribeRooms = chatRooms.Select(roomId => Groups.Add(connectionId, roomId));

            chatRooms.ForEach(roomId => _chatUserProvider.SubscribeConnectionToChatRoom(userId, connectionId, roomId));

            return Task.WhenAll(subscribeRooms);
        }
        public Task UnsubscribeConnectionFromChatRooms(int userId, string connectionId, IList<string> chatRooms)
        {
            var unsubscribeRooms = chatRooms.Select(roomId => Groups.Remove(connectionId, roomId));
            return Task.WhenAll(unsubscribeRooms);
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

            _chatService.OnConnected(UserId, Context.ConnectionId);
            //var isFirstConnection = !_chatUserProvider.UserHasActiveConnections(UserId);
            //if (isFirstConnection)
            //{
            //    _chatUserProvider.AddUserToRoom(UserId, ChatRoom.DefaultRoomId);
            //}

            //_chatUserProvider.RegisterUserConnection(UserId, Context.ConnectionId);

            //var affectedChatRooms = _chatUserProvider.GetUsersChatRooms(UserId);
            //SubscribeConnectionToChatRooms(UserId, Context.ConnectionId, affectedChatRooms);

            //if (isFirstConnection)
            //{
            //    affectedChatRooms.ForEach(UpdateChatRoomUsers);
            //    UserStatusUpdate(UserId, UserStatus.Online, ChatRoom.DefaultRoomId);
            //}

            //_chatUserProvider.SaveAll();

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _chatService.OnDisconnected(UserId, Context.ConnectionId);
            //var affectedChatRooms = _chatUserProvider.GetUsersChatRooms(UserId);
            //UnsubscribeConnectionFromChatRooms(UserId, Context.ConnectionId, affectedChatRooms);

            //bool isLastConnection;
            //_chatUserProvider.DeleteConnection(UserId, Context.ConnectionId, out isLastConnection);

            //if (isLastConnection)
            //{
            //    affectedChatRooms.ForEach(UpdateChatRoomUsers);
            //    UserStatusUpdate(UserId, UserStatus.Offline, ChatRoom.DefaultRoomId);
            //    _chatUserProvider.RemoveUserFromRoom(UserId, ChatRoom.DefaultRoomId);
            //}

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