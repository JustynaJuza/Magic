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
        bool UserHasActiveConnections(int userId);

        User GetUser(int userId);

        void RegisterUserConnection(int userId, string connectionId);

        int GetUserConnectionCount(int userId);

        IList<string> GetUsersChatRooms(int userId);

        void SubscribeConnectionToChatRoom(int userId, string connectionId, string roomId);

        void AddUserToRoom(int userId, string roomId);

        void RemoveUserFromRoom(int userId, string roomId);

        void DeleteConnection(int userId, string connectionId, out bool isLastConnection);

        IEnumerable<int> FindUsersByUserName(IList<string> userNames, out IEnumerable<string> invalidUserNames);

        ChatRoom GetChatRoomForUsers(IEnumerable<int> userIds);

        ChatRoom CreateChatRoomForUsers(IEnumerable<int> userIds);

        IList<ChatUserViewModel> GetAvailableUsers(int userId);

        IEnumerable<ChatRoom> GetUserChatRooms(int userId, bool exceptDefaultRoom = false);
        //IList<ChatRoom> GetChatRoomsWithUser(int userId);
        IList<ChatRoom> GetUserGameRooms(int userId, bool exceptDefaultRoom = false);
        string GetExistingChatRoomIdForUsers(string[] recipientNames);
        void CreateChatRoom(string roomId, bool isGameRoom, bool isPrivate, IEnumerable<string> recipientNames);
        IEnumerable<ChatUserViewModel> GetChatRoomUsers(string roomId);
        void SaveMessage(User sender, string roomId, string messageText, DateTime timeSent);
        void UserStatusUpdate(int userId, UserStatus status);
        void SubscribeConnectionToChatRoom(string roomId, string connectionId, int userId);
        List<string> UnsubscribeChatRoom(int userId, string roomId);
        IEnumerable<string> SubscribeUsersChatRooms(int userId, string connectionId);
        IEnumerable<string> SubscribeActiveConnections(string roomId, int userId);
        IEnumerable<string> SubscribeGameChat(int userId, string connectionId, string roomId);
        bool UnsubscribeGameChat(int userId, string connectionId, string roomId);
        //int OnConnected(int userId, string connectionId);
        void DeleteConnection(string connectionId);
        void RemoveInactiveConnections();
        void SaveAll();
    }

    public class ChatDataProvider : IChatDataProvider
    {
        private readonly IDbContext _context;
        public ChatDataProvider(IDbContext context)
        {
            _context = context;
        }

        public bool UserHasActiveConnections(int userId)
        {
            var connectionCount = GetUserConnectionCount(userId);
            return connectionCount != 0;
        }

        public User GetUser(int userId)
        {
            return _context.Read<User>().Find(userId);
        }


        public void RegisterUserConnection(int userId, string connectionId)
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

        public IList<string> GetUsersChatRooms(int userId)
        {
            return _context.Set<ChatRoomUser>()
                .Where(x => x.UserId == userId)
                .Select(x => x.ChatRoomId)
                .Distinct()
                .ToList();
        }

        public void SubscribeConnectionToChatRoom(int userId, string connectionId, string roomId)
        {
            _context.Insert(new ChatRoomConnection
            {
                ChatRoomId = roomId,
                ConnectionId = connectionId,
                UserId = userId
            });
        }

        public void AddUserToRoom(int userId, string roomId)
        {
            var existingUser = _context.Read<ChatRoomUser>().Find(userId, roomId);
            if (existingUser == null)
            {
                _context.Insert(new ChatRoomUser
                {
                    ChatRoomId = roomId,
                    UserId = userId
                });
            }
        }

        public void RemoveUserFromRoom(int userId, string roomId)
        {
            var chatRoomUser = _context.Read<ChatRoomUser>().Find(userId, roomId);
            _context.Delete(chatRoomUser);
        }

        public void SubscribeConnectionToChatRoom(string roomId, string connectionId, int userId)
        {
            _context.Insert(new ChatRoomConnection
            {
                ChatRoomId = roomId,
                ConnectionId = connectionId,
                UserId = userId
            });
        }

        public void UserStatusUpdate(int userId, UserStatus status)
        {
            var user = GetUser(userId);
            user.Status = status;
        }


        // Returns user's friends and currently online users to populate the available user list when you want to create a chat room with multiple user's.
        public IList<ChatUserViewModel> GetAvailableUsers(int userId)
        {
            var userWithRelations = _context.Read<User>()
                .Include(x => x.Relations.OfType<UserRelationFriend>().Select(r => r.RelatedUser))
                .Find(userId);

            var usersFriends = userWithRelations.Relations
                .Select(x => new ChatUserViewModel(x.RelatedUser))
                .OrderBy(x => x.UserName);

            var defaultChatRoom = _context.Set<ChatRoom>().Where(r => r.Id == ChatRoom.DefaultRoomId).Include(r => r.Connections.Select(u => u.User)).First();
            var activeUsers = defaultChatRoom.GetActiveUserList().OrderBy(u => u.UserName).ToList();

            activeUsers.Remove(activeUsers.First(u => u.Id == userId));
            return usersFriends.Union(activeUsers, new ChatUserViewModel_UserComparer()).ToList();

        }

        // Returns non-game chat rooms the user has any connections subscribed to to open them on page load.
        public IEnumerable<ChatRoom> GetUserChatRooms(int userId, bool exceptDefaultRoom = false)
        {
            var chatRooms = _context.Set<ChatRoomConnection>()
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
        //public IList<ChatRoom> GetChatRoomsWithUser(int userId)
        //{
        //    return _context.Set<ChatRoomUser>()
        //        .Where(x => x.UserId == userId)
        //        .Select(x => x.ChatRoom)
        //        .ToList();
        //}

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

        public IEnumerable<int> FindUsersByUserName(IList<string> userNames, out IEnumerable<string> invalidUserNames)
        {
            var distinctNames = userNames.Distinct();
            var users = _context.Set<User>().Where(x => userNames.Contains(x.UserName));

            invalidUserNames = distinctNames.Except(users.Select(x => x.UserName));

            return users.Select(x => x.Id);
        }

        public ChatRoom GetChatRoomForUsers(IEnumerable<int> userIds)
        {
            var chatRoom = _context
                .Set<ChatRoom>()
                .Include(x => x.Users.Select(y => y.User))
                .Where(x => x.Id != ChatRoom.DefaultRoomId)
                .Where(x => !x.IsGameRoom)
                .FirstOrDefault(x => x.Users.All(y => userIds.Contains(y.UserId)));

            return chatRoom ?? CreateChatRoomForUsers(userIds);
        }

        public ChatRoom CreateChatRoomForUsers(IEnumerable<int> userIds)
        {
            var chatRoom = new ChatRoom
            {
                Id = Guid.NewGuid().ToString(),
                IsGameRoom = false,
                IsPrivate = true,
                Users = userIds.Select(userId => new ChatRoomUser { UserId = userId }).ToList()
            };

            _context.Insert(chatRoom);
            return chatRoom;
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
            var recipients = recipientNames.Distinct().Select(userName => _context.Set<User>().FirstOrDefault(u => u.UserName == userName)).ToList();

            foreach (var user in recipients)
            {
                AddUserToRoom(user.Id, chatRoom.Id);
                SubscribeActiveConnections(chatRoom.Id, user.Id);
            }
            _context.SaveChanges();
        }

        public IEnumerable<ChatUserViewModel> GetChatRoomUsers(string roomId)
        {
            var users = _context.Set<ChatRoomUser>()
                .Where(x => x.ChatRoomId == roomId)
                .Select(x => x.User);

            return users.Project<User, ChatUserViewModel>();
            //return chatRoom.IsPrivate ? chatRoom.GetUserList() : chatRoom.GetActiveUserList();
        }

        public void SaveMessage(User sender, string roomId, string messageText, DateTime timeSent)
        {
            var chatRoomUserIds = _context.Set<ChatRoomUser>()
                .Where(x => x.ChatRoomId == roomId)
                .Select(x => x.UserId)
                .ToList();

            var message = new ChatMessage
            {
                LogId = roomId,
                Sender = sender,
                Message = messageText,
                TimeSent = timeSent
            };

            if (roomId != ChatRoom.DefaultRoomId)
            {
                message.RecipientNotifications = chatRoomUserIds.Select(recipientId => new ChatMessageNotification
                {
                    RecipientId = recipientId
                }).ToList();
            }

            _context.Insert(message);
            _context.SaveChanges();
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

        public List<string> UnsubscribeChatRoom(int userId, string roomId)
        {
            var userConnections = _context.Set<ChatRoomConnection>()
                .Where(x => x.ChatRoomId == roomId)
                .Where(x => x.UserId == userId);

            _context.Set<ChatRoomConnection>().RemoveRange(userConnections);
            _context.SaveChanges();

            return userConnections.Select(c => c.ConnectionId).ToList();
        }

        public IEnumerable<string> SubscribeUsersChatRooms(int userId, string connectionId)
        {
            var activeChatRoomIds = GetUsersChatRooms(userId);
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
                .Find(roomId);

            if (chatRoom == null)
            {
                var game = _context.Read<Game>().Include(x => x.Players.Select(y => y.User)).Find(roomId);
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
            var connection = _context.Read<ChatRoomConnection>().Find(connectionId, userId);

            var hasOtherGameConnections = _context.Set<UserConnection>().Count(c => c.GameId == connection.ChatRoomId && c.UserId == userId) > 1;
            if (hasOtherGameConnections) return false;

            //_gameConnectionManager.LeaveGame(connection);

            UnsubscribeChatRoom(userId, roomId);
            System.Diagnostics.Debug.WriteLine("Unsubscribing " + roomId);
            return true;
        }

        //public int OnConnected(int userId, string connectionId)
        //{
        //    var foundUser = _context.Read<User>().Include(x => x.Connections).Find(userId);

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

        //    SubscribeConnectionToChatRoom(ChatRoom.DefaultRoomId, connectionId, userId);

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

        public void DeleteConnection(int userId, string connectionId, out bool isLastConnection)
        {
            var connection = _context.Read<UserConnection>().Include(x => x.User.Connections).Find(connectionId, userId);
            isLastConnection = !connection.User.Connections.Any();

            //if (!string.IsNullOrWhiteSpace(connection.GameId))
            //{
            //    UnsubscribeGameChat(connection.UserId, connection.Id, connection.GameId);
            //}

            _context.Delete(connection);
        }

        public void DeleteConnection(string connectionId)
        {
            var connection = _context.Read<UserConnection>().Include(x => x.User.Connections).Find(connectionId);
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
            foreach (var connection in _context.Set<UserConnection>())
            {
                _context.Delete(connection);
            }
            //_context.Database.ExecuteSqlCommand("TRUNCATE TABLE [AspNetUserConnections]");
        }
        #endregion REMOVE INACTIVE USERS
    }
}