using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Enums;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using WebGrease.Css.Extensions;

namespace Juza.Magic.Hubs
{
    public interface IChatServiceFactory
    {
        IChatService Create(
            int userId,
            string connectionId,
            IHubCallerConnectionContext<IChatHub> clients,
            IGroupManager groups);
    }

    public class ChatServiceFactory : IChatServiceFactory
    {
        private readonly IChatDataProvider _chatUserProvider;

        public ChatServiceFactory(IChatDataProvider chatUserProvider)
        {
            _chatUserProvider = chatUserProvider;
        }

        public IChatService Create(
            int userId,
            string connectionId,
            IHubCallerConnectionContext<IChatHub> clients,
            IGroupManager groups)
        {
            return new ChatService(_chatUserProvider, userId, connectionId, clients, groups);
        }
    }

    public interface IChatService
    {
        void OnConnected();
        void OnDisconnected();
    }

    public class ChatService : IChatService
    {
        private readonly IChatDataProvider _chatUserProvider;
        private readonly int _userId;
        private readonly string _connectionId;
        private readonly IHubCallerConnectionContext<IChatHub> _clients;
        private readonly IGroupManager _groups;

        public ChatService(IChatDataProvider chatUserProvider,
            int userId,
            string connectionId,
            IHubCallerConnectionContext<IChatHub> clients,
            IGroupManager groups)
        {
            _chatUserProvider = chatUserProvider;
            _userId = userId;
            _connectionId = connectionId;
            _clients = clients;
            _groups = groups;
        }

        public void OnConnected()
        {
            var isFirstConnection = !_chatUserProvider.UserHasActiveConnections(_userId);
            if (isFirstConnection)
            {
                _chatUserProvider.AddUserToRoom(_userId, ChatRoom.DefaultRoomId);
            }

            _chatUserProvider.RegisterUserConnection(_userId, _connectionId);

            var affectedChatRooms = _chatUserProvider.GetUsersChatRooms(_userId);
            SubscribeConnectionToChatRooms(affectedChatRooms);

            if (isFirstConnection)
            {
                affectedChatRooms.ForEach(UpdateChatRoomUsers);
                UserStatusUpdate(UserStatus.Online, ChatRoom.DefaultRoomId);
            }

            _chatUserProvider.SaveAll();
            System.Diagnostics.Debug.WriteLine("Connected: " + _connectionId);
        }

        public void OnDisconnected()
        {
            var affectedChatRooms = _chatUserProvider.GetUsersChatRooms(_userId);
            UnsubscribeConnectionFromChatRooms(affectedChatRooms);

            bool isLastConnection;
            _chatUserProvider.DeleteConnection(_userId, _connectionId, out isLastConnection);

            if (isLastConnection)
            {
                affectedChatRooms.ForEach(UpdateChatRoomUsers);
                UserStatusUpdate(UserStatus.Offline, ChatRoom.DefaultRoomId);
                _chatUserProvider.RemoveUserFromRoom(_userId, ChatRoom.DefaultRoomId);
            }

            _chatUserProvider.SaveAll();

            System.Diagnostics.Debug.WriteLine("Disconnected: " + _connectionId);
        }

        public void UpdateChatRoomUsers(string roomId)
        {
            var chatRoomUsers = _chatUserProvider.GetChatRoomUsers(roomId);
            _clients.Group(roomId).updateChatRoomUsers(Json.Encode(chatRoomUsers), roomId);
        }

        public void Send(string messageText, string roomId)
        {
            var sender = _chatUserProvider.GetUser(_userId);
            var timeSent = DateTime.Now;
            _clients.Group(roomId).addMessage(roomId, timeSent.ToString("HH:mm:ss"), sender.UserName, sender.ColorCode, messageText);

            _chatUserProvider.SaveMessage(_userId, roomId, messageText, timeSent);
        }

        public void UserStatusUpdate(UserStatus status, string roomId)
        {
            var user = _chatUserProvider.GetUser(_userId);
            _chatUserProvider.UserStatusUpdate(_userId, status);

            _clients.Group(roomId)
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
            await SubscribeChatRoom(ChatRoom.DefaultRoomId);

            var connectionCount = _chatUserProvider.GetUserConnectionCount(userId);
            if (connectionCount == 1)
            {
                UserStatusUpdate(UserStatus.Online, ChatRoom.DefaultRoomId);
            }
        }

        public Task SubscribeChatRoom(string roomId)
        {
            var addToGroup = _groups.Add(_connectionId, roomId);
            _chatUserProvider.SubscribeChatRoom(roomId, _connectionId, _userId);

            if (roomId != ChatRoom.DefaultRoomId)
            {
                UpdateChatRoomUsers(roomId);
            }

            return addToGroup;
        }

        public Task UnsubscribeChatRoom(string roomId)
        {
            var connections = _chatUserProvider.UnsubscribeChatRoom(_userId, roomId);

            _clients.Clients(connections).closeChatRoom(roomId);

            var unsubscribeUser = connections.Select(connectionId => _groups.Remove(connectionId, roomId));

            return Task.WhenAll(unsubscribeUser);
        }
        //public Task SubscribeActiveChatRooms(int userId, string _connectionId)
        //{
        //    var activeChatRooms = _chatUserProvider.SubscribeUsersChatRooms(userId, _connectionId);

        //    var subscribeRooms = activeChatRooms.Select(roomId => _groups.Add(_connectionId, roomId));
        //    return Task.WhenAll(subscribeRooms);
        //}

        public Task SubscribeConnectionToChatRooms(IList<string> chatRooms)
        {
            var subscribeRooms = chatRooms.Select(roomId => _groups.Add(_connectionId, roomId));

            chatRooms.ForEach(roomId => _chatUserProvider.SubscribeConnectionToChatRoom(_userId, _connectionId, roomId));

            return Task.WhenAll(subscribeRooms);
        }
        public Task UnsubscribeConnectionFromChatRooms(IList<string> chatRooms)
        {
            var unsubscribeRooms = chatRooms.Select(roomId => _groups.Remove(_connectionId, roomId));
            return Task.WhenAll(unsubscribeRooms);
        }


        public Task SubscribeActiveConnections(IEnumerable<string> connections, string roomId)
        {
            var subscribeUser = connections.Select(connectionId => _groups.Add(connectionId, roomId));

            return Task.WhenAll(subscribeUser);
        }

        public async Task SubscribeGameChat(string roomId)
        {
            var activeUnsubscribedConnections = _chatUserProvider.SubscribeGameChat(_userId, _connectionId, roomId).ToList();

            // Send info message on joining group - also opens the chat tab for joining user.
            if (activeUnsubscribedConnections.Any())
            {
                var subscribe = SubscribeActiveConnections(activeUnsubscribedConnections, roomId);

                var chatRoom = _chatUserProvider.GetChatRoom(roomId);
                _clients.Caller.addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), chatRoom.Name, chatRoom.TabColorCode, " Welcome back!", true);

                await subscribe;
            }
            else
            {
                var user = _chatUserProvider.GetUser(_userId);
                _clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, " entered the game.", true);
            }
        }

        public void UnsubscribeGameChat(string roomId)
        {
            _chatUserProvider.UnsubscribeGameChat(_userId, _connectionId, roomId);
            //TODO
            //_gameConnectionManager.LeaveGame(connection);

            UnsubscribeChatRoom(roomId);
            System.Diagnostics.Debug.WriteLine("Unsubscribing " + roomId);

            // Sent info message on leaving group.
            var user = _chatUserProvider.GetUser(_userId);
            _clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, (" left the game."), true);

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