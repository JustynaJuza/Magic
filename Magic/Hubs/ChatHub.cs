using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Magic.Models;
using Magic.Models.DataContext;
using System.Threading.Tasks;
using System.Web.Helpers;
using Magic.Helpers;
using Magic.Controllers;
using System.Data.Entity;

namespace Magic.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public const string DefaultRoomId = "default";

        #region CHAT INIT
        public static IList<ChatUserViewModel> GetAvailableUsers(string userId)
        {
            using (var context = new MagicDbContext())
            {
                var user = context.Users.Include(u => u.Relations.Select(r => r.RelatedUser)).First(u => u.Id == userId);
                var usersFriends = user.GetFriendsList().OrderBy(u => u.UserName);

                var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(u => u.User)).First(r => r.Id == DefaultRoomId);
                var activeUsers = chatRoom.GetActiveUserList().OrderBy(u => u.UserName).ToList();

                activeUsers.Remove(activeUsers.First(u => u.Id == userId));
                return usersFriends.Union(activeUsers, new ChatUserViewModel_UserComparer()).ToList();
            }
        }

        public static IList<ChatRoom> GetUserChatRooms(string userId)
        {
            using (var context = new MagicDbContext())
            {
                //var userId = Context.User.Identity.GetUserId();
                var chatRooms = context.ChatRoomConnections.Select(rc => rc.ChatRoom).Distinct().Where(r => r.Connections.Any(c => c.UserId == userId) && r.Id != DefaultRoomId); //.OrderByDescending(r => r.Name == DefaultRoomId);
                return chatRooms.ToList();
                //Clients.Caller.loadActiveChatRooms(Json.Encode(chatRooms));
            }
        }

        public void AddUserToRoom(string roomId, string userId)
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

        public string GetChatRoom(string[] recipientNames)
        {
            using (var context = new MagicDbContext())
            {
                var recipients = new List<User>();
                foreach (var userName in recipientNames.Distinct())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserName == userName);
                    if (user == null)
                    {
                        Clients.Caller.addMessage(DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "User " + userName + " was not found!");
                    }
                    else
                    {
                        recipients.Add(user);
                    }
                }

                var recipientIds = recipients.Select(r => r.Id);

                var chatRoom = context.ChatRooms.Where(r => r.IsPrivate).ToList().FirstOrDefault(r => r.OnlySpecifiedUsersInRoom(recipientIds));

                if (chatRoom == null)
                {
                    return String.Empty;
                }

                foreach (var user in recipients)
                {
                    SubscribeActiveConnections(chatRoom.Id, user.Id);
                }
                return chatRoom.Id;

                //Clients.Caller.loadChatRoom(chatRoom.Id, chatRoom.Name, recipientColors, recipientNames, Json.Encode(chatRoom.Log.GetUserMessages(userId)));
                //var userId = Context.User.Identity.GetUserId();
                //return ViewRenderer.RenderPartialView("~/Views/Shared/_ChatRoomPartial.cshtml", (ChatRoomViewModel)chatRoom.GetViewModel(userId));
            }
        }

        public void CreateChatRoom(string roomId = null, bool isGameRoom = false, bool isPrivate = false, string[] recipientNames = null)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = new ChatRoom(roomId, isGameRoom, isPrivate);
                context.Insert(chatRoom);

                if (isPrivate)
                {
                    var recipients = recipientNames.Distinct().Select(userName => context.Users.FirstOrDefault(u => u.UserName == userName)).ToList();

                    foreach (var user in recipients)
                    {
                        AddUserToRoom(chatRoom.Id, user.Id);
                        SubscribeActiveConnections(chatRoom.Id, user.Id);
                    }
                }
            }
        }

        public void GetChatRoomLog(string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var chatLog = context.ChatLogs.Find(roomId);

                Clients.Caller.loadChatLog(Json.Encode(chatLog));
            }
        }

        public async void SubscribeChatRoom(string roomId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoomConnection = new ChatRoomUserConnection
                {
                    ChatRoomId = roomId,
                    ConnectionId = Context.ConnectionId,
                    UserId = Context.User.Identity.GetUserId()
                };
                context.Insert(chatRoomConnection);
            }

            await Groups.Add(Context.ConnectionId, roomId);

            UpdateChatRoomUsers(roomId);
        }

        public void UnsubscribeChatRoom(string roomId, string connectionId = null)
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var user = context.Users.Find(userId);

                if (string.IsNullOrWhiteSpace(connectionId))
                {
                    var userConnections = context.ChatRoomConnections.Where(c => c.ChatRoomId == roomId && c.UserId == userId);
                    Clients.Clients(userConnections.Select(c => c.ConnectionId).ToList()).closeChatRoom(roomId);

                    context.ChatRoomConnections.RemoveRange(userConnections);
                    context.SaveChanges();
                }
                else
                {
                    var connection = context.ChatRoomConnections.Find(connectionId, userId, roomId);
                    context.Delete(connection, true);
                }
            }
        }

        public void UpdateChatRoomUsers(string roomId, bool callerOnly = false)
        {
            using (var context = new MagicDbContext())
            {
                List<ChatUserViewModel> chatUsers;
                if (roomId == DefaultRoomId)
                {
                    var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(u => u.User)).First(r => r.Id == roomId);
                    chatUsers = chatRoom.GetActiveUserList().ToList();
                }
                else
                {
                    var chatRoom = context.ChatRooms.Include(r => r.Users.Select(u => u.User)).First(r => r.Id == roomId);
                    chatUsers = chatRoom.GetUserList().ToList();
                }

                if (callerOnly)
                {
                    Clients.Caller.updateChatRoomUsers(Json.Encode(chatUsers), roomId);
                }
                else
                {
                    Clients.All.updateChatRoomUsers(Json.Encode(chatUsers), roomId);
                }
            }
        }
        #endregion CHAT INIT

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

                var chatRoom = context.ChatRooms.Find(roomId);

                foreach (var recipientId in chatRoom.Users.Select(u => u.UserId).Except(new string[] { userId }))
                {
                    message.Recipients.Add(new MessageRecipient()
                    {
                        RecipientId = recipientId
                    });
                }

                chatRoom.AddMessageToLog(message);
                context.InsertOrUpdate(chatRoom, true);

                Clients.Group(roomId).addMessage(roomId, message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
            }
        }

        public static void UserStatusBroadcast(string userId, UserStatus status, string roomId = DefaultRoomId)
        {
            using (var context = new MagicDbContext())
            {
                var message = new ChatMessage
                {
                    Sender = context.Users.Find(userId),
                    Message = UserStatusBroadcastMessage(status)
                };
                message.Sender.Status = status;
                context.InsertOrUpdate(message.Sender, true);


                var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                if (roomId.Length > 0)
                {
                    chatHubContext.Clients.Group(roomId).addMessage(roomId, message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                }
                else
                {
                    chatHubContext.Clients.All.addMessage(roomId, message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
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
                case UserStatus.Ready: return " is ready for action.";
                case UserStatus.Unready: return " seems to be not prepared!";
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
                    Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
                        "- no such user found, have you misspelled the name?", recipientName, "#696969");
                    return false;
                }

                // Valid recipient found.
                if (recipient.Status == UserStatus.Offline)
                {
                    // Valid recipient but is offline, alert sender.
                    Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
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
        public void SubscribeActiveChatRooms(string connectionId, string userId)
        {
            //var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            using (var context = new MagicDbContext())
            {
                var activeChatRoomIds = context.ChatRoomConnections.Where(rc => rc.UserId == userId && rc.ChatRoomId != DefaultRoomId).Select(rc => rc.ChatRoomId).Distinct();

                foreach (var roomId in activeChatRoomIds)
                {
                    context.ChatRoomConnections.Add(new ChatRoomUserConnection
                    {
                        ChatRoomId = roomId,
                        ConnectionId = connectionId,
                        UserId = userId
                    });

                    Groups.Add(connectionId, roomId);
                }
                context.SaveChanges();
            }
        }

        public static void SubscribeActiveConnections(string roomId, string userId)
        {
            using (var context = new MagicDbContext())
            {
                var activeConnectionIds = context.Connections.Where(c => c.UserId == userId).Select(c => c.Id);
                var subscribedConnectionIds = context.ChatRoomConnections.Where(crc => crc.UserId == userId && crc.ChatRoomId == roomId).Select(crc => crc.ConnectionId);
                var unsubscribedConnectionIds = activeConnectionIds.Except(subscribedConnectionIds);

                foreach (var connectionId in unsubscribedConnectionIds)
                {
                    context.ChatRoomConnections.Add(new ChatRoomUserConnection
                    {
                        ChatRoomId = roomId,
                        ConnectionId = connectionId,
                        UserId = userId
                    });

                    var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    chatHubContext.Groups.Add(connectionId, roomId);
                }
                context.SaveChanges();
            }
        }

        public static void AddConnectionsToRoomGroup(IList<string> connectionIds, string roomId)
        {
            var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            foreach (var connectionId in connectionIds)
            {
                chatHubContext.Groups.Add(connectionId, roomId);
            }
        }

        public async void ToggleGameChatSubscription(string roomId, bool isPrivate = false, string[] recipientNames = null, bool activate = true)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = context.ChatRooms.Find(roomId);
                var userId = Context.User.Identity.GetUserId();

                if (chatRoom == null)
                {
                    CreateChatRoom(roomId, true, isPrivate, recipientNames);
                    System.Diagnostics.Debug.WriteLine("Joining " + roomId);
                }
                else if (chatRoom.IsPrivate == false || chatRoom.Users.Any(u => u.UserId == userId))
                {
                    SubscribeChatRoom(roomId);
                    System.Diagnostics.Debug.WriteLine("Joining " + roomId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Can't join private game " + roomId);
                    return;
                }

                // Sent info message on joining and leaving group.
                var user = context.Users.Find(userId);
                Clients.Group(roomId).addMessage(DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, (activate ? " entered the game." : " left the game."));
            

                //var currentConnection = context.Connections.Find(Context.ConnectionId);

                //if (activate)
                //{
                //    // Set the current connection as game connection.
                //    currentConnection.GameId = gameId;
                //    context.InsertOrUpdate(currentConnection, true);
                //    await GameHub.JoinGame(Context.ConnectionId, gameId);
                //}
                //else
                //{
                //    currentConnection.GameId = null;
                // Closing main connection to a game, remove chat subscribtion from all other connections.
                //foreach (var connection in message.Sender.Connections)
                //{
                //    Groups.Remove(connection.Id, gameId);
                //}
                //}

                //var message = new ChatMessage
                //{
                //    Sender = user,
                //    Message = activate ? " entered the game." : " left the game."
                //};
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

                SubscribeChatRoom(DefaultRoomId);
                SubscribeActiveChatRooms(Context.ConnectionId, userId);

                if (foundUser.Connections.Count == 1)
                {
                    // If this is the user's only connection broadcast a chat info.
                    UserStatusBroadcast(userId, UserStatus.Online);
                }
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
            await Task.Delay(1000);
            using (var context = new MagicDbContext())
            {
                var connection = context.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);

                if (connection != null)
                {
                    UnsubscribeChatRoom(DefaultRoomId, connection.Id);
                    if (connection.User.Connections.Count == 1)
                    {
                        // If this is the user's last connection broadcast a chat info.
                        UserStatusBroadcast(connection.User.Id, UserStatus.Offline);
                        UpdateChatRoomUsers(DefaultRoomId);
                    }

                    //if (connection.GetType() == typeof(ApplicationUserGameConnection))
                    //{
                    //    //ToggleGameChatSubscription(connection.ChatRoomId, false);
                    //    //GameHub.DisplayUserLeft(connection.User.UserName, connection.ChatRoomId);
                    //    GameHub.LeaveGame((ApplicationUserGameConnection) connection);
                    //}

                    System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);

                    context.Delete(connection, true);
                }
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