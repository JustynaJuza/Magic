using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Magic.Models.Chat;
using Magic.Models.Helpers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Magic.Models;
using Magic.Models.DataContext;
using System.Threading.Tasks;
using System.Web.Helpers;
using Magic.Controllers;

namespace Magic.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        #region CHAT SERVICES
        // Returns user's friends and currently online users to populate the available user list when you want to create a chat room with multiple user's.
        public static IList<ChatUserViewModel> GetAvailableUsers(string userId)
        {
            using (var context = new MagicDbContext())
            {
                var userWithRelations = context.Users.Where(u => u.Id == userId).Include(u => u.Relations.Select(r => r.RelatedUser)).First();
                var usersFriends = userWithRelations.GetFriendsList().OrderBy(u => u.UserName);

                var defaultChatRoom = context.ChatRooms.Where(r => r.Id == ChatRoom.DefaultRoomId).Include(r => r.Connections.Select(u => u.User)).First();
                var activeUsers = defaultChatRoom.GetActiveUserList().OrderBy(u => u.UserName).ToList();

                activeUsers.Remove(activeUsers.First(u => u.Id == userId));
                return usersFriends.Union(activeUsers, new ChatUserViewModel_UserComparer()).ToList();
            }
        }

        // Returns non-game chat rooms the user has any connections subscribed to to open them on page load.
        public static IList<ChatRoom> GetUserChatRooms(string userId, bool exceptDefaultRoom = false)
        {
            using (var context = new MagicDbContext())
            {
                var chatRooms = context.ChatRoomConnections.Where(rc => rc.UserId == userId).Select(rc => rc.ChatRoom).Distinct().Where(r => !r.IsGameRoom).ToList();
                if (exceptDefaultRoom)
                {
                    chatRooms.Remove(chatRooms.FirstOrDefault(r => r.Id == ChatRoom.DefaultRoomId));
                }
                return chatRooms;
            }
        }

        // Returns chat rooms in which user's online visibility is listed.
        public static IList<ChatRoom> GetChatRoomsWithUser(string userId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRooms = context.ChatRoomUsers.Where(ru => ru.UserId == userId).Select(rc => rc.ChatRoom).ToList();
                return chatRooms;
            }
        }
        
        // Returns game chat rooms the user has any connections subscribed to to open them on page load.
        public static IList<ChatRoom> GetUserGameRooms(string userId, bool exceptDefaultRoom = false)
        {
            using (var context = new MagicDbContext())
            {
                return context.ChatRoomConnections.Where(rc => rc.UserId == userId).Select(rc => rc.ChatRoom).Distinct().Where(r => r.IsGameRoom).ToList();
            }
        }

        // Returns the chat room's id if a private chat room exists for given users only.
        public string GetExistingChatRoomIdForUsers(string[] recipientNames)
        {
            using (var context = new MagicDbContext())
            {
                var recipients = new List<User>();
                foreach (var userName in recipientNames.Distinct())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserName == userName);
                    if (user == null)
                    {
                        Clients.Caller.addMessage(ChatRoom.DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "User " + userName + " was not found!");
                    }
                    else
                    {
                        recipients.Add(user);
                    }
                }

                var recipientIds = recipients.Select(r => r.Id);
                var chatRoom = context.ChatRooms.Where(r => r.IsPrivate).ToList().FirstOrDefault(r => r.OnlySpecifiedUsersInRoom(recipientIds));

                return chatRoom == null ? String.Empty : chatRoom.Id;
            }
        }

        // Create new chat room with given settings and for specific users only if it's private.
        public void CreateChatRoom(string roomId = null, bool isGameRoom = false, bool isPrivate = false, IList<string> recipientNames = null)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = new ChatRoom(roomId)
                {
                    IsGameRoom = isGameRoom,
                    IsPrivate = isPrivate,
                    Name = (isGameRoom ? "Game" : null),
                    TabColorCode = (isGameRoom ? string.Empty.AssignRandomColorCode() : null)
                };
                context.Insert(chatRoom);

                if (!isPrivate) return;

                // TODO: check how recipients behave after chacking chatroom existance and if thee can be any null exception
                var recipients = recipientNames.Distinct().Select(userName => context.Users.FirstOrDefault(u => u.UserName == userName)).ToList();

                foreach (var user in recipients)
                {
                    AddUserToRoom(chatRoom.Id, user.Id);
                    SubscribeActiveConnections(chatRoom.Id, user.Id);
                }
            }
        }

        public void UpdateChatRoomUsers(string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = context.ChatRooms.Find(roomId);

                List<ChatUserViewModel> chatUsers;
                if (chatRoom.IsPrivate)
                {
                    context.Entry(chatRoom).Collection(r => r.Users).Query().Include(u => u.User).Load(); //ChatRooms.Include(r => r.Users.Select(u => u.User)).First(r => r.Id == roomId)));
                    chatUsers = chatRoom.GetUserList().ToList();
                }
                else
                {
                    context.Entry(chatRoom).Collection(r => r.Connections).Query().Include(u => u.User).Load();
                    //var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(u => u.User)).First(r => r.Id == roomId);
                    chatUsers = chatRoom.GetActiveUserList().ToList();
                }

                Clients.Group(roomId).updateChatRoomUsers(Json.Encode(chatUsers), roomId);
            }
        }
        #endregion CHAT SERVICES

        #region CHAT MESSAGE HANDLING
        public void Send(string messageText, string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var sender = context.Users.Find(userId);
                var message = new ChatMessage(messageText)
                {
                    Sender = sender,
                    Message = messageText
                };

                var chatRoom = context.ChatRooms.Include(r => r.Users.Select(u => u.User)).First(r => r.Id == roomId);
                foreach (var user in chatRoom.Users)
                {
                    SubscribeActiveConnections(chatRoom.Id, user.UserId);
                }

                Clients.Group(roomId).addMessage(roomId, message.TimeSend.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                
                chatRoom.AddMessageToLog(message);
                context.InsertOrUpdate(chatRoom);

                foreach (var notification in chatRoom.Users.Where(u => u.User.Status == UserStatus.Offline).Select(u => new ChatMessageNotification
                    {
                        RecipientId = u.UserId,
                        MessageId = message.Id,
                        LogId = message.LogId
                    }))
                {
                    context.Insert(notification);
                }
            }
        }

        public static void UserStatusUpdate(string userId, UserStatus status, string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var user = context.Users.Find(userId);
                user.Status = status;
                context.InsertOrUpdate(user, true);

                var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                if (roomId.Length > 0)
                {
                    chatHubContext.Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, UserStatusBroadcastMessage(status));
                }
                else
                {
                    chatHubContext.Clients.All.addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, UserStatusBroadcastMessage(status));
                }
            }
        }

        private static string UserStatusBroadcastMessage(UserStatus status)
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

        private bool ValidateMessage(string messageText, string recipientName, out User recipient)
        {
            recipient = null;

            if (messageText.Length <= 0) return false; // No message text at all.
            if (recipientName.Length <= 0) return true; // Group chat message.

            using (var context = new MagicDbContext())
            {
                // Look for recipient.
                recipient = context.Users.FirstOrDefault(u => u.UserName == recipientName);
                if (recipient == null)
                {
                    // Recipient included but invalid, alert sender.
                    Clients.Caller.addMessage(ChatRoom.DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
                        "- no such user found, have you misspelled the name?", recipientName, "#696969");
                    return false;
                }

                // Valid recipient found.
                if (recipient.Status == UserStatus.Offline)
                {
                    // Valid recipient but is offline, alert sender.
                    Clients.Caller.addMessage(ChatRoom.DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
                        "is currently offline and unable to receive messages.", recipient.UserName,
                        recipient.ColorCode);
                    return false;
                }

                return true; // Valid message for recipient.
            }
        }

        private string DecodeRecipient(string messageText, out User recipient)
        {
            var recipientName = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipientName.Length > 0)
            {
                recipientName = recipientName.Substring(1);
            }
            // Try to find recipient.
            using (var context = new MagicDbContext())
            {
                recipient = context.Users.FirstOrDefault(u => u.UserName == recipientName);
            }
            return recipientName;
        }
        #endregion CHAT MESSAGE HANDLING

        #region MANAGE CHAT & GAME GROUPS
        public static void AddUserToRoom(string roomId, string userId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoomAllowedUser = new ChatRoomUser
                {
                    ChatRoomId = roomId,
                    UserId = userId
                };
                context.Insert(chatRoomAllowedUser);
            }
        }

        public async void SubscribeChatRoom(string roomId)
        {
            var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            var addToGroup = chatHubContext.Groups.Add(Context.ConnectionId, roomId);

            using (var context = new MagicDbContext())
            {
                var chatRoomConnection = new ChatRoomConnection
                {
                    ChatRoomId = roomId,
                    ConnectionId = Context.ConnectionId,
                    UserId = Context.User.Identity.GetUserId()
                };
                context.Insert(chatRoomConnection);
            }

            await addToGroup;

            if (roomId != ChatRoom.DefaultRoomId)
            {
                UpdateChatRoomUsers(roomId);
            }
        }

        public void UnsubscribeChatRoom(string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();

                var userConnections = context.ChatRoomConnections.Where(c => c.ChatRoomId == roomId && c.UserId == userId);
                var userConnectionIds = userConnections.Select(c => c.ConnectionId).ToList();
                Clients.Clients(userConnectionIds).closeChatRoom(roomId);

                context.ChatRoomConnections.RemoveRange(userConnections);
                context.SaveChanges();

                Task.WhenAll(userConnectionIds.Select(connectionId => Groups.Remove(connectionId, roomId)).ToArray());
            }
        }

        public void SubscribeActiveChatRooms(string connectionId, string userId)
        {
            using (var context = new MagicDbContext())
            {
                var activeChatRoomIds = context.ChatRoomConnections.Where(rc => rc.UserId == userId && rc.ChatRoomId != ChatRoom.DefaultRoomId).Select(rc => rc.ChatRoomId).Distinct();

                foreach (var roomId in activeChatRoomIds)
                {
                    var chatRoomConnection = new ChatRoomConnection
                    {
                        ChatRoomId = roomId,
                        ConnectionId = connectionId,
                        UserId = userId
                    };

                    Groups.Add(connectionId, roomId);
                    context.Insert(chatRoomConnection);
                }
            }
        }
        
        public static void SubscribeActiveConnections(string roomId, string userId)
        {
            using (var context = new MagicDbContext())
            {
                var activeConnectionIds = context.Connections.Where(c => c.UserId == userId).Select(c => c.Id);
                var subscribedConnectionIds = context.ChatRoomConnections.Where(crc => crc.UserId == userId && crc.ChatRoomId == roomId).Select(crc => crc.ConnectionId);
                var unsubscribedConnectionIds = activeConnectionIds.Except(subscribedConnectionIds);

                if (!unsubscribedConnectionIds.Any()) return;

                var groupsProcessed = new List<Task>();
                foreach (var connectionId in unsubscribedConnectionIds)
                {
                    var chatRoomConnection = new ChatRoomConnection
                    {
                        ChatRoomId = roomId,
                        ConnectionId = connectionId,
                        UserId = userId
                    };

                    var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    groupsProcessed.Add(chatHubContext.Groups.Add(connectionId, roomId));
                    context.Insert(chatRoomConnection);
                }

                Task.WhenAll(groupsProcessed.ToArray());
            }
        }

        public void SetAsGameConnection(string gameId)
        {
            var userId = Context.User.Identity.GetUserId();
            using (var context = new MagicDbContext())
            {
                var connection = context.Connections.Find(Context.ConnectionId, userId);
                connection.GameId = gameId;
                context.InsertOrUpdate(connection, true);
            }
        }

        public bool SubscribeGameChat(string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = context.ChatRooms.Find(roomId);
                var userId = Context.User.Identity.GetUserId();
                bool isExistingUser = false;

                if (chatRoom == null)
                {
                    var game = context.Games.Find(roomId);
                    CreateChatRoom(roomId, true, game.IsPrivate, game.Players.Select(p => p.User.UserName).ToList());
                    //AddUserToRoom(roomId, userId);
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
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("Joining " + roomId);
                SetAsGameConnection(roomId);
                SubscribeActiveConnections(roomId, userId);

                // Send info message on joining group - also opens the chat tab for joining user.
                if (isExistingUser)
                {
                    Clients.Caller.addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), chatRoom.Name, chatRoom.TabColorCode, " Welcome back!", true);
                }
                else {
                    var user = context.Users.Find(userId);
                    Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, " entered the game.", true);
                }
                return true;
            }
        }

        public void UnsubscribeGameChat()
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var connection = context.Connections.Find(Context.ConnectionId, userId);

                var hasOtherGameConnections = context.Connections.Count(c => c.GameId == connection.GameId && c.UserId == userId) > 1;

                if (hasOtherGameConnections) return;

                GameHub.LeaveGame(connection);

                var roomId = connection.GameId;
                UnsubscribeChatRoom(roomId);
                System.Diagnostics.Debug.WriteLine("Unsubscribing " + roomId);

                // Sent info message on leaving group.
                var user = context.Users.Find(userId);
                Clients.Group(roomId).addMessage(roomId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, (" left the game."), true);
            }
        }
        #endregion MANAGE CHAT & GAME GROUPS

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var foundUser = context.Users.Find(userId);

                var connection = new UserConnection
                {
                    Id = Context.ConnectionId,
                    UserId = userId
                };
                context.Connections.Add(connection);
                context.SaveChanges();

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
            }

            System.Diagnostics.Debug.WriteLine("Connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            DeleteConnection();
            return base.OnDisconnected();
        }

        private async void DeleteConnection()
        {
            await Task.Delay(1500);

            using (var context = new MagicDbContext())
            {
                var connection = context.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
                if (connection == null) return;

                if (!string.IsNullOrWhiteSpace(connection.GameId))
                {
                    UnsubscribeGameChat();
                }

                System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);

                var isLastConnection = connection.User.Connections.Count == 1;
                if (!isLastConnection)
                {
                    context.Delete(connection, true);
                    return;
                }

                // If this is the user's last connection update chat room users.
                var userId = connection.User.Id;
                var chatRooms = GetUserChatRooms(userId);

                context.Delete(connection, true);

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
        }
        #endregion CONNECTION STATUS UPDATE

        #region CHATLOG HANDLING
        // This function is called by schedule from Global.asax and uses the static context to save recent chat messages.
        //public static bool SaveChatLogToDatabase(ChatLog currentLog)
        //{
        //    ChatLog todayLog = context.ChatLogs.Find(currentLog.DateCreated);//context.Set<ChatLog>().AsNoTracking().FirstOrDefault(c => c.DateCreated == currentLog.DateCreated);
        //    if (todayLog != null)
        //    {
        //        todayLog.AppendMessages(currentLog.Messages);
        //        return context.InsertOrUpdate(todayLog, true);
        //    }

        //    // Create new log for Today.
        //    todayLog = new ChatLog
        //    {
        //        Messages = currentLog.Messages
        //    };
        //    return context.Insert(todayLog);
        //}
        #endregion CHATLOG HANDLING

        #region REMOVE INACTIVE USERS
        public static void RemoveInactiveConnections()
        {
            using (var context = new MagicDbContext())
            {
                foreach (var connection in context.Connections)
                {
                    context.Delete(connection);
                }
            }
            //context.Database.ExecuteSqlCommand("TRUNCATE TABLE [AspNetUserConnections]");
        }
        #endregion REMOVE INACTIVE USERS
    }
}