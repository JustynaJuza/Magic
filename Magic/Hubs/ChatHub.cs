using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Magic.Models.Chat;
using Magic.Models.Extensions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Magic.Models;
using Magic.Models.DataContext;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNet.SignalR.Hubs;

namespace Magic.Hubs
{
    public interface IChatHub
    {
        void addMessage (string roomId, string time, string sender, string senderColor, string message, bool activateTabAfterwards = false);
        void closeChatRoom(string roomId);
        void updateChatRoomUsers (string chatUsersList, string roomId);
    }

    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private readonly IDbContext _context;
        private readonly IGameConnectionManager _gameConnectionManager;
        private readonly IChatUserProvider _chatUserProvider;

        public ChatHub(IDbContext context,
            IGameConnectionManager gameConnectionManager,
            IChatUserProvider chatUserProvider)
        {
            _context = context;
            _gameConnectionManager = gameConnectionManager;
            _chatUserProvider = chatUserProvider;
        }


        public void UpdateChatRoomUsers(string roomId)
        {
            var chatRoomUsers = _chatUserProvider.GetChatRoomUsers(roomId);
            Clients.Group(roomId).updateChatRoomUsers(Json.Encode(chatRoomUsers), roomId);

        }

        public void Send(string messageText, string roomId)
        {
            var sender = _chatUserProvider.GetUser(Context.User.Identity.GetUserId());
            var timeSent = DateTime.Now;
                Clients.Group(roomId).addMessage(roomId, timeSent.ToString("HH:mm:ss"), sender.UserName, sender.ColorCode, messageText);

            _chatUserProvider.SaveMessage(Context.User.Identity.GetUserId(), roomId, messageText, timeSent);
        }

        public void UserStatusUpdate(string userId, UserStatus status, string roomId)
        {
            var user = _chatUserProvider.GetUser(Context.User.Identity.GetUserId());
            _chatUserProvider.UserStatusUpdate(userId, status);

            if (roomId.Length > 0)
            {
                Clients.Group(roomId)
                    .addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, UserStatusBroadcastMessage(status));
            }
            else
            {
                Clients.All
                    .addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, UserStatusBroadcastMessage(status));
            } 
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

        public async void SubscribeChatRoom(string roomId)
        {
            var addToGroup = Groups.Add(Context.ConnectionId, roomId);
            _chatUserProvider.SubscribeChatRoom(roomId, Context.ConnectionId, Context.User.Identity.GetUserId());
            await addToGroup;

            if (roomId != ChatRoom.DefaultRoomId)
            {
                UpdateChatRoomUsers(roomId);
            }
        }

        public Task UnsubscribeChatRoom(string roomId)
        {
            var connections = _chatUserProvider.UnsubscribeChatRoom(Context.User.Identity.GetUserId(), roomId);
            
                Clients.Clients(connections).closeChatRoom(roomId);

                var unsubscribeUser = connections.Select(connectionId => Groups.Remove(connectionId, roomId));

                return Task.WhenAll(unsubscribeUser);
        }

        public Task SubscribeActiveChatRooms(string connectionId, string userId)
        {
            var activeChatRooms = _chatUserProvider.SubscribeActiveChatRooms(connectionId, userId);

            var subscribeUser = activeChatRooms.Select(roomId => Groups.Add(connectionId, roomId));
                
            return Task.WhenAll(subscribeUser);
        }

        public Task SubscribeActiveConnections(IEnumerable<string> connections, string roomId)
        {
            var subscribeUser = connections.Select(connectionId => Groups.Add(connectionId, roomId));
                
            return Task.WhenAll(subscribeUser);
        }

        public async Task SubscribeGameChat(string roomId)
        {
            var activeUnsubscribedConnections = _chatUserProvider.SubscribeGameChat(Context.User.Identity.GetUserId(), Context.ConnectionId, roomId).ToList();
            
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
                    var user = _chatUserProvider.GetUser(Context.User.Identity.GetUserId());
                    Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, " entered the game.", true);
                }
        }

        public void UnsubscribeGameChat()
        {
                //TODO
                _gameConnectionManager.LeaveGame(connection);

                var roomId = connection.GameId;
                Unsubsc ribeChatRoom(roomId);
                System.Diagnostics.Debug.WriteLine("Unsubscribing " + roomId);

                // Sent info message on leaving group.
                var user = _context.Users.Find(userId);
                Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, (" left the game."), true);
            }
        }
        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();
            var foundUser = _context.Query<User>().Find(userId);

            var connection = new UserConnection
            {
                Id = Context.ConnectionId,
                UserId = userId
            };
            _context.Query<UserConnection>().Add(connection);
            _context.SaveChanges();

            SubscribeChatRoom(ChatRoom.DefaultRoomId);
            if (foundUser.Connections.Count == 1)
            {
                // If this is the user's only connection broadcast a chat info.
                UserStatusUpdate(userId, UserStatus.Online, ChatRoom.DefaultRoomId);
            }

            UpdateChatRoomUsers(ChatRoom.DefaultRoomId);
            foreach (var chatRoom in GetChatRoomsWithUser(userId))
            {
                UpdateChatRoomUsers(chatRoom.Id);
            }

            SubscribeActiveChatRooms(Context.ConnectionId, userId);
            // TODO: What when: user disconnects, other user has private chat tab open, user connects again - chat tab is unsubscribed, typing message does not notify receiving user?
            // Solution: use message notifications for that - partially implemented.


            System.Diagnostics.Debug.WriteLine("Connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            DeleteConnection();
            return base.OnDisconnected(stopCalled);
        }

        public void DeleteConnection(string connectionId)
        {
            var connection = _context.Read<UserConnection>().Include(x => x.User.Connections).FindOrFetchEntity(connectionId);
            if (connection == null) return;

            if (!string.IsNullOrWhiteSpace(connection.GameId))
            {
                UnsubscribeGameChat();
            }

            System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);

            var isLastConnection = connection.User.Connections.Count == 1;
            if (!isLastConnection)
            {
                _context.Delete(connection);
                return;
            }

            // If this is the user's last connection update chat room users.
            var userId = connection.User.Id;
            var chatRooms = GetUserChatRooms(userId);

            _context.Delete(connection, true);

            UserStatusUpdate(userId, UserStatus.Offline, ChatRoom.DefaultRoomId);
            foreach (var chatRoom in chatRooms)
            {
                UpdateChatRoomUsers(chatRoom.Id);
            }

            //if (connection.GetType() == typeof(ApplicationUserGameConnection))
            //{
            //    //SubscribeGameChat(connection.ChatRoomId, false);
            //    //GameHub.DisplayUserLeft(connection.User.UserName, connection.ChatRoomId);
            //    GameHub.LeaveGame((ApplicationUserGameConnection) connection);
            //}
        }
        #endregion CONNECTION STATUS UPDATE

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

        #region REMOVE INACTIVE USERS
        public void RemoveInactiveConnections()
        {
            foreach (var connection in _context.Query<UserConnection>())
            {
                _context.Delete(connection);
            }
            //_context.Database.ExecuteSqlCommand("TRUNCATE TABLE [AspNetUserConnections]");
        }
        #endregion REMOVE INACTIVE USERS
    }
}