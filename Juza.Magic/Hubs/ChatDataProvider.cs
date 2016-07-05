using Juza.Magic.Models;
using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Enums;
using Juza.Magic.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Juza.Magic.Hubs
{
    public interface IChatDataProvider
    {
        User GetUser(int userId);
        void AddUserConnection(int userId, string connectionId);
        int GetUserConnectionCount(int userId);
        IList<ChatUserViewModel> GetAvailableUsers(int userId);
        IList<ChatRoom> GetUserChatRooms(int userId, bool exceptDefaultRoom = false);
        IList<ChatRoom> GetChatRoomsWithUser(int userId);
        IList<ChatRoom> GetUserGameRooms(int userId, bool exceptDefaultRoom = false);
        string GetExistingChatRoomIdForUsers(string[] recipientNames);
        void CreateChatRoom(string roomId, bool isGameRoom, bool isPrivate, IEnumerable<string> recipientNames);
        IEnumerable<ChatUserViewModel> GetChatRoomUsers(string roomId);
        void SaveMessage(int userId, string roomId, string messageText, DateTime timeSent);
        void UserStatusUpdate(int userId, UserStatus status);
        void AddUserToRoom(string roomId, int userId);
        void SubscribeChatRoom(string roomId, string connectionId, int userId);
        List<string> UnsubscribeChatRoom(int userId, string roomId);
        IEnumerable<string> SubscribeActiveChatRooms(int userId, string connectionId);
        IEnumerable<string> SubscribeActiveConnections(string roomId, int userId);
        IEnumerable<string> SubscribeGameChat(int userId, string connectionId, string roomId);
        bool UnsubscribeGameChat(int userId, string connectionId, string roomId);
        //int OnConnected(int userId, string connectionId);
        void DeleteConnection(string connectionId);
        void RemoveInactiveConnections();
        ChatRoom GetChatRoom(string roomId);
        void SaveAll();
    }

    public class ChatDataProvider : IChatDataProvider
    {
        private readonly IDbContext _context;

        public void AddUserConnection(int userId, string connectionId)
        {
            _context.Insert(new UserConnection
            {
                Id = connectionId,
                UserId = userId
            });
        }

        public int GetUserConnectionCount(int userId)
        {
            return _context.Set<UserConnection>()
                .Count(x => x.UserId == userId);
        }


        public ChatDataProvider(IDbContext context)
        {
            _context = context;
        }

        //TODO
        public ChatRoom GetChatRoom(string roomId)
        {
            return _context.Read<ChatRoom>().FindOrFetchEntity(roomId);
        }

        public User GetUser(int userId)
        {
            return _context.Read<User>().FindOrFetchEntity(userId);
        }

        // Returns user's friends and currently online users to populate the available user list when you want to create a chat room with multiple user's.
        public IList<ChatUserViewModel> GetAvailableUsers(int userId)
        {
            var userWithRelations = _context.Read<User>()
                .Include(x => x.Relations.OfType<UserRelationFriend>().Select(r => r.RelatedUser))
                .FindOrFetchEntity(userId);

            var usersFriends = userWithRelations.Relations
                .Select(x => new ChatUserViewModel(x.RelatedUser))
                .OrderBy(x => x.UserName);

            var defaultChatRoom = _context.Set<ChatRoom>().Where(r => r.Id == ChatRoom.DefaultRoomId).Include(r => r.Connections.Select(u => u.User)).First();
            var activeUsers = defaultChatRoom.GetActiveUserList().OrderBy(u => u.UserName).ToList();

            activeUsers.Remove(activeUsers.First(u => u.Id == userId));
            return usersFriends.Union(activeUsers, new ChatUserViewModel_UserComparer()).ToList();

        }

        // Returns non-game chat rooms the user has any connections subscribed to to open them on page load.
        public IList<ChatRoom> GetUserChatRooms(int userId, bool exceptDefaultRoom = false)
        {
            var chatRooms = _context.Query<ChatRoomConnection>()
                .Where(rc => rc.UserId == userId)
                .Select(rc => rc.ChatRoom)
                .Distinct()
                .Where(r => !r.IsGameRoom).ToList();

            if (exceptDefaultRoom)
            {
                chatRooms.Remove(chatRooms.FirstOrDefault(r => r.Id == ChatRoom.DefaultRoomId));
            }

            return chatRooms;
        }

        // Returns chat rooms in which user's online visibility is listed.
        public IList<ChatRoom> GetChatRoomsWithUser(int userId)
        {
            return _context.Set<ChatRoomUser>()
                .Where(x => x.UserId == userId)
                .Select(x => x.ChatRoom)
                .ToList();
        }

        // Returns game chat rooms the user has any connections subscribed to to open them on page load.
        public IList<ChatRoom> GetUserGameRooms(int userId, bool exceptDefaultRoom = false)
        {
            return _context.Set<ChatRoomConnection>()
                .Where(x => x.UserId == userId)
                .Select(x => x.ChatRoom)
                .Where(r => r.IsGameRoom)
                .Distinct()
                .ToList();
        }

        // Returns the chat room's id if a private chat room exists for given users only.
        public string GetExistingChatRoomIdForUsers(string[] recipientNames)
        {
            var distinctNames = recipientNames.Distinct();
            var recipients = _context.Set<User>().Where(x => distinctNames.Contains(x.UserName));

            var missingUserNames = recipients.Select(x => x.UserName).Except(recipientNames);
            foreach (var userName in missingUserNames)
            {
                //Clients.Caller.addMessage(ChatRoom.DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "User " + userName + " was not found!");
            }

            var recipientIds = recipients.Select(r => r.Id);
            var chatRoom = _context.Set<ChatRoom>().Where(r => r.IsPrivate).ToList().FirstOrDefault(r => r.OnlySpecifiedUsersInRoom(recipientIds));

            return chatRoom == null ? String.Empty : chatRoom.Id;
        }

        // Create new chat room with given settings and for specific users only if it's private.
        public void CreateChatRoom(string roomId, bool isGameRoom, bool isPrivate, IEnumerable<string> recipientNames)
        {
            var chatRoom = new ChatRoom
            {
                Id = roomId ?? Guid.NewGuid().ToString(),
                IsGameRoom = isGameRoom,
                IsPrivate = isPrivate,
                Name = (isGameRoom ? "Game" : null),
                TabColorCode = (isGameRoom ? string.Empty.AssignRandomColorCode() : null)
            };
            _context.Insert(chatRoom);

            if (!isPrivate) return;

            // TODO: check how recipients behave after chacking chatroom existance and if thee can be any null exception
            var recipients = recipientNames.Distinct().Select(userName => _context.Query<User>().FirstOrDefault(u => u.UserName == userName)).ToList();

            foreach (var user in recipients)
            {
                AddUserToRoom(chatRoom.Id, user.Id);
                SubscribeActiveConnections(chatRoom.Id, user.Id);
            }
            _context.SaveChanges();
        }

        public IEnumerable<ChatUserViewModel> GetChatRoomUsers(string roomId)
        {
            var chatRoom = _context.Read<ChatRoom>()
                .Include(x => x.Users.Select(y => y.User))
                .Include(x => x.Connections.Select(y => y.User))
                .FindOrFetchEntity(roomId);

            return chatRoom.IsPrivate ? chatRoom.GetUserList() : chatRoom.GetActiveUserList();
        }

        public void SaveMessage(int userId, string roomId, string messageText, DateTime timeSent)
        {
            var chatRoom = _context.Read<ChatRoom>().Include(r => r.Users.Select(u => u.User)).FindOrFetchEntity(roomId);
            foreach (var user in chatRoom.Users)
            {
                SubscribeActiveConnections(chatRoom.Id, user.UserId);
            }

            var sender = chatRoom.Users.First(x => x.UserId == userId).User;
            var message = new ChatMessage
            {
                Sender = sender,
                Message = messageText,
                TimeSent = timeSent
            };

            foreach (var notification in chatRoom.Users.Where(u => u.User.Status == UserStatus.Offline).Select(u => new ChatMessageNotification
            {
                RecipientId = u.UserId,
                MessageId = message.Id,
                LogId = message.LogId
            }))
            {
                _context.Insert(notification);
            }

            chatRoom.AddMessageToLog(message);
            _context.SaveChanges();
        }

        public void UserStatusUpdate(int userId, UserStatus status)
        {
            var user = _context.Read<User>().FindOrFetchEntity(userId);
            user.Status = status;
        }

        private string DecodeRecipient(string messageText, out User recipient)
        {
            var recipientName = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipientName.Length > 0)
            {
                recipientName = recipientName.Substring(1);
            }
            // Try to find recipient.
            using (var _context = new MagicDbContext())
            {
                recipient = _context.Users.FirstOrDefault(u => u.UserName == recipientName);
            }
            return recipientName;
        }

        public void AddUserToRoom(string roomId, int userId)
        {
            var chatRoomAllowedUser = new ChatRoomUser
            {
                ChatRoomId = roomId,
                UserId = userId
            };
            _context.Insert(chatRoomAllowedUser);
        }

        public void SubscribeChatRoom(string roomId, string connectionId, int userId)
        {
            _context.Insert(new ChatRoomConnection
            {
                ChatRoomId = roomId,
                ConnectionId = connectionId,
                UserId = userId
            });
        }

        public List<string> UnsubscribeChatRoom(int userId, string roomId)
        {
            var userConnections = _context.Set<ChatRoomConnection>()
                .Where(x => x.ChatRoomId == roomId)
                .Where(x => x.UserId == userId);

            _context.Set<ChatRoomConnection>().RemoveRange(userConnections);
            _context.SaveChanges();

            return userConnections.Select(c => c.ConnectionId).ToList();
        }

        public IEnumerable<string> SubscribeActiveChatRooms(int userId, string connectionId)
        {
            var activeChatRoomIds = _context.Set<ChatRoomConnection>()
                .Where(rc => rc.UserId == userId)
                .Where(rc => rc.ChatRoomId != ChatRoom.DefaultRoomId)
                .Select(rc => rc.ChatRoomId).Distinct()
                .ToList();

            foreach (var roomId in activeChatRoomIds)
            {
                var chatRoomConnection = new ChatRoomConnection
                {
                    ChatRoomId = roomId,
                    ConnectionId = connectionId,
                    UserId = userId
                };

                _context.Insert(chatRoomConnection);
            }

            return activeChatRoomIds;
        }

        public IEnumerable<string> SubscribeActiveConnections(string roomId, int userId)
        {
            var activeConnectionIds = _context.Set<UserConnection>().Where(c => c.UserId == userId).Select(c => c.Id);
            var subscribedConnectionIds = _context.Set<ChatRoomConnection>().Where(crc => crc.UserId == userId && crc.ChatRoomId == roomId).Select(crc => crc.ConnectionId);
            var unsubscribedConnectionIds = activeConnectionIds.Except(subscribedConnectionIds);

            if (!unsubscribedConnectionIds.Any())
                return Enumerable.Empty<string>();

            foreach (var connectionId in unsubscribedConnectionIds)
            {
                var chatRoomConnection = new ChatRoomConnection
                {
                    ChatRoomId = roomId,
                    ConnectionId = connectionId,
                    UserId = userId
                };
                _context.Insert(chatRoomConnection);
            }

            return unsubscribedConnectionIds;
        }

        public IEnumerable<string> SubscribeGameChat(int userId, string connectionId, string roomId)
        {
            var isExistingUser = false;

            var chatRoom = _context.Read<ChatRoom>()
                .Include(x => x.Users)
                .Include(x => x.Connections)
                .FindOrFetchEntity(roomId);

            if (chatRoom == null)
            {
                var game = _context.Read<Game>().Include(x => x.Players.Select(y => y.User)).FindOrFetchEntity(roomId);
                CreateChatRoom(roomId, true, game.IsPrivate, game.Players.Select(p => p.User.UserName));
            }
            else if (chatRoom.IsUserInRoom(userId))
            {
                isExistingUser = true;
            }
            else if (chatRoom.IsUserAllowedToJoin(userId))
            {
                //AddUserToRoom(roomId, userId);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Can't join private game " + roomId);
            }

            System.Diagnostics.Debug.WriteLine("Joining " + roomId);

            //SetAsGameConnection(roomId);
            var connection = _context.Set<UserConnection>().Find(connectionId, userId);
            connection.GameId = roomId;
            _context.InsertOrUpdate(connection);

            var unsubscribedConnectionIds = Enumerable.Empty<string>();
            if (!isExistingUser)
            {
                unsubscribedConnectionIds = SubscribeActiveConnections(roomId, userId);
            }
            _context.SaveChanges();

            return unsubscribedConnectionIds;
        }

        public bool UnsubscribeGameChat(int userId, string connectionId, string roomId)
        {
            var connection = _context.Read<ChatRoomConnection>().FindOrFetchEntity(connectionId, userId);

            var hasOtherGameConnections = _context.Set<UserConnection>().Count(c => c.GameId == connection.ChatRoomId && c.UserId == userId) > 1;
            if (hasOtherGameConnections) return false;

            //_gameConnectionManager.LeaveGame(connection);

            UnsubscribeChatRoom(userId, roomId);
            System.Diagnostics.Debug.WriteLine("Unsubscribing " + roomId);
            return true;
        }

        //public int OnConnected(int userId, string connectionId)
        //{
        //    var foundUser = _context.Read<User>().Include(x => x.Connections).FindOrFetchEntity(userId);

        //    if (foundUser.Connections.All(x => x.Id != connectionId))
        //    {
        //        var connection = new UserConnection
        //        {
        //            Id = connectionId,
        //            UserId = userId
        //        };
        //        _context.Query<UserConnection>().Add(connection);
        //        _context.SaveChanges();
        //    }

        //    SubscribeChatRoom(ChatRoom.DefaultRoomId, connectionId, userId);

        //    //UpdateChatRoomUsers(ChatRoom.DefaultRoomId);
        //    //foreach (var chatRoom in GetChatRoomsWithUser(userId))
        //    //{
        //    //    UpdateChatRoomUsers(chatRoom.Id);
        //    //}

        //    SubscribeActiveChatRooms(connectionId, userId);
        //    // TODO: What when: user disconnects, other user has private chat tab open, user connects again - chat tab is unsubscribed, typing message does not notify receiving user?
        //    // Solution: use message notifications for that - partially implemented.
        //    return foundUser.Connections.Count;
        //}


        public void DeleteConnection(string connectionId)
        {
            var connection = _context.Read<UserConnection>().Include(x => x.User.Connections).FindOrFetchEntity(connectionId);
            if (connection == null) return;

            if (!string.IsNullOrWhiteSpace(connection.GameId))
            {
                UnsubscribeGameChat(connection.UserId, connection.Id, connection.GameId);
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

            _context.Delete(connection);

            UserStatusUpdate(userId, UserStatus.Offline);
            // TODO
            //UserStatusUpdate(userId, UserStatus.Offline, ChatRoom.DefaultRoomId);
            //foreach (var chatRoom in chatRooms)
            //{
            //    UpdateChatRoomUsers(chatRoom.Id);
            //}

            //if (connection.GetType() == typeof(ApplicationUserGameConnection))
            //{
            //    //SubscribeGameChat(connection.ChatRoomId, false);
            //    //GameHub.DisplayUserLeft(connection.User.UserName, connection.ChatRoomId);
            //    GameHub.LeaveGame((ApplicationUserGameConnection) connection);
            //}
        }

        public void SaveAll()
        {
            _context.SaveChanges();
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