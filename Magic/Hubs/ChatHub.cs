using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Magic.Models;
using Magic.Models.DataContext;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Magic.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public const string DefaultRoomId = "default";
        //private static MagicDbContext context = new MagicDbContext();

        #region CHAT INIT
        public void ListChatRooms(string userId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRooms = context.ChatRoom_Connections.Where(rc => rc.ChatRoom.UserIsInRoom(userId)).Select(rc => rc.ChatRoom);

                Json.Encode(chatRooms);
            }
        }

        public async void SubscribeChatRoom(string roomId = DefaultRoomId)
        {
            using (var context = new MagicDbContext())
            {
                context.ChatRoom_Connections.Add(new ChatRoom_ApplicationUserConnection
                {
                    ChatRoomId = roomId,
                    ConnectionId = Context.ConnectionId,
                    UserId = Context.User.Identity.GetUserId()
                });
                context.SaveChanges();
            }

            await Groups.Add(Context.ConnectionId, roomId);

            UpdateChatRoomUsers(roomId);
        }

        public void UpdateChatRoomUsers(string roomId = DefaultRoomId, bool callerOnly = false)
        {
            //var room = context.ChatRooms.Find(roomId);

            //var chatUsers = new List<ChatUserViewModel>();
            //foreach (var connection in room.Connections.Distinct(new ApplicationUserConnection_UserComparer())){
            //    chatUsers.Add(new ChatUserViewModel(connection.User));
            //}
            using (var context = new MagicDbContext())
            {
                try
                {
                    var chatRoomConnections = context.ChatRoom_Connections.Where(rc => rc.ChatRoomId == roomId).Select(rc => rc.Connection).ToList();
                    var chatRoomUsers = chatRoomConnections.Distinct(new ApplicationUserConnection_UserComparer()).Select(c => c.User);

                    var chatUsers = new List<ChatUserViewModel>();
                    foreach (var user in chatRoomUsers)
                    {
                        chatUsers.Add(new ChatUserViewModel(user));
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
                catch (Exception ex)
                {
                    var x = ex.ToString();
                }
            }
        }

        public void AddChatRoom()
        {

        }
        #endregion CHAT INIT

        #region CHAT MESSAGE HANDLING
        public void Send(string messageText, string roomId = DefaultRoomId, string recipientName = "")
        {
            //ApplicationUser recipient;
            //var messageIsValid = ValidateMessage(messageText, recipientName, out recipient);

            using (var context = new MagicDbContext())
            {
                var recipient = context.Users.FirstOrDefault(u => u.UserName == recipientName);
                //if (messageIsValid)
                //{
                    var userId = Context.User.Identity.GetUserId();
                    var sender = context.Users.Find(userId);
                    var message = new ChatMessage(messageText)
                    {
                        SenderId = userId,
                        Recipient = recipient,
                        Message = messageText
                    };

                    ChatRoom chatRoom;
                    if (!string.IsNullOrWhiteSpace(roomId))
                    {
                        chatRoom = context.ChatRooms.Find(roomId);
                    }
                    else{
                        chatRoom = //context.ChatRooms.First(r => r.IsPrivate && r.OnlySpecifiedUsersInRoom(new string[] { recipient.Id, userId })) ??
                            // Create new chat room if no private conversation found.
                            new ChatRoom()
                            {
                                IsPrivate = true,
                                AllowedUserIds = { recipient.Id, userId },
                                TabColorCodes = { sender.ColorCode, recipient.ColorCode }
                            };
                        context.InsertOrUpdate(chatRoom);

                        SubscribeActiveConnections(chatRoom.Id, userId);
                        SubscribeActiveConnections(chatRoom.Id, recipient.Id);
                    }

                    // Add message to chatlog in correct room.
                    chatRoom.Log.Messages.Add(message);
                    context.InsertOrUpdate(chatRoom);

                    // Depending on settings, use callback method to update clients.
                    if (recipient == null)
                    {
                        if (roomId.Length > 0)
                        {
                            //AddMessageToChatLog(message, roomName);
                            Clients.Group(roomId).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                        }
                        else
                        {
                            //AddMessageToChatLog(message);
                            Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                        }
                    }
                    else
                    {
                        // Get message text after username and following space.
                        //message.Message = messageText.Substring(recipient.UserName.Length + 2);
                        // Set message recipient.
                        message.Recipient = recipient;

                        // Send message to all recipient and all sender connections.
                        foreach (var connection in message.Recipient.Connections)
                        {
                            Clients.Client(connection.Id).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"),
                                message.Sender.UserName, message.Sender.ColorCode, message.Message, message.Recipient.UserName, message.Recipient.ColorCode);
                        }
                        foreach (var connection in message.Sender.Connections)
                        {
                            Clients.Client(connection.Id).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"),
                                message.Sender.UserName, message.Sender.ColorCode, message.Message, message.Recipient.UserName, message.Recipient.ColorCode);
                        }
                    }
                //}
            }
        }

        public static void UserStatusBroadcast(string userId, UserStatus status, string roomName = DefaultRoomId)
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
                if (roomName.Length > 0)
                {
                    chatHubContext.Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                }
                else
                {
                    chatHubContext.Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
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

        private bool ValidateMessage(string messageText, string recipientName, out ApplicationUser recipient)
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

        private string DecodeRecipient(string messageText, out ApplicationUser recipient)
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
                var activeChatRoomIds = context.ChatRoom_Connections.Where(rc => rc.UserId == userId && rc.ChatRoomId != DefaultRoomId).Select(rc => rc.ChatRoomId).Distinct();

                foreach (var roomId in activeChatRoomIds)
                {
                    context.ChatRoom_Connections.Add(new ChatRoom_ApplicationUserConnection
                    {
                        ChatRoomId = roomId,
                        ConnectionId = connectionId,
                        UserId = userId
                    });

                    Groups.Add(connectionId, roomId);
                }
                context.SaveChanges();
            }

            //var connection = context.Connections.Find(connectionId);

            //    var chatRooms = context.ChatRooms.Where(r => r.Connections.Any(c => c.User.Id == connection.User.Id));
            //    foreach(var chatRoom in chatRooms){
            //        chatRoom.Connections.Add(connection);
            //        context.Update(chatRoom);

            //        chatHubContext.Groups.Add(connection.Id, chatRoom.Id);
            //    }

            //else
            //{
            //    var chatRooms = context.ChatRooms.Where(r => r.UserConnections.Any(c => c.Id == connection.Id));
            //    foreach(var chatRoom in chatRooms){
            //        chatRoom.UserConnections.Remove(connection);
            //        context.Update(chatRoom);

            //        chatHubContext.Groups.Remove(connection.Id, chatRoom.Id);
            //    }
            //}
        }

        public void SubscribeActiveConnections(string roomId, string userId)
        {
            using (var context = new MagicDbContext())
            {
                var activeConnectionIds = context.Connections.Where(c => c.UserId == userId && c.Id != Context.ConnectionId).Select(c => c.Id);

                foreach (var connectionId in activeConnectionIds)
                {
                    context.ChatRoom_Connections.Add(new ChatRoom_ApplicationUserConnection
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

        //public async void ToggleGameChatSubscription(string gameId, bool activate)
        //{
        //    var userId = Context.User.Identity.GetUserId();
        //    var foundUser = context.Users.Find(userId);
        //    var message = new ChatMessage
        //    {
        //        Sender = foundUser,
        //        Message = activate ? " entered the game." : " left the game."
        //    };

        //    if (activate)
        //    {
        //        // Set the current connection as game connection.
        //        var currentConnection = foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
        //        var gameConnection = new ApplicationUserGameConnection
        //        {
        //                Id = currentConnection.Id,
        //                User = foundUser,
        //                //ChatRoom = context.ChatRooms.Find(gameId) ?? new ChatRoom { Id = gameId },
        //                Game = context.Games.Find(gameId) ?? new Game { Id = gameId }
        //            };

        //        foundUser.Connections.Remove(currentConnection);
        //        context.Update(foundUser, true);
        //        foundUser.GameConnections.Add(gameConnection);
        //        context.Update(foundUser, true);

        //        // Await to join the group on main connection so the joining user get's the info message.
        //        await GameHub.JoinGame(gameConnection.Id, gameId);

        //        // Subscribe all other chat connections.
        //        foreach (var connection in foundUser.Connections)
        //        {
        //            Groups.Add(connection.Id, gameId);
        //        }
        //    }
        //    else
        //    {
        //        // Closing main connection to a game, remove chat subscribtion from all other connections.
        //        foreach (var connection in message.Sender.Connections)
        //        {
        //            Groups.Remove(connection.Id, gameId);
        //        }
        //    }

        //    System.Diagnostics.Debug.WriteLine("Joining " + gameId);
        //    // Sent info message on joining and leaving group.
        //    await Clients.Group(gameId).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        //}
        #endregion MANAGE CHAT & GAME GROUPS

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var foundUser = context.Users.Find(userId);

                var connection = new ApplicationUserConnection
                {
                    Id = Context.ConnectionId,
                    UserId = userId
                };
                context.Connections.Add(connection);
                context.SaveChanges();

                SubscribeActiveChatRooms(Context.ConnectionId, userId);
                SubscribeChatRoom(DefaultRoomId);

                if (foundUser.Connections.Count == 1)
                {
                    // If this is the user's only connection broadcast a chat info.
                    UserStatusBroadcast(userId, UserStatus.Online);
                }
            }

            System.Diagnostics.Debug.WriteLine("Connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            using (var context = new MagicDbContext())
            {
                var userId = Context.User.Identity.GetUserId();
                var foundUser = context.Users.Find(userId);

                if (foundUser.Connections.Count == 1)
                {
                    // If this is the user's only connection broadcast a chat info.
                    UserStatusBroadcast(userId, UserStatus.Online);
                }
            }

            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            using (var context = new MagicDbContext())
            {
                var connection = context.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);

                if (connection != null)
                {
                    if (connection.User.Connections.Count == 1)
                    {
                        // If this is the user's last connection broadcast a chat info.
                        UserStatusBroadcast(connection.User.Id, UserStatus.Offline);
                    }

                    //if (connection.GetType() == typeof(ApplicationUserGameConnection))
                    //{
                    //    //ToggleGameChatSubscription(connection.ChatRoomId, false);
                    //    //GameHub.DisplayUserLeft(connection.User.UserName, connection.ChatRoomId);
                    //    GameHub.LeaveGame((ApplicationUserGameConnection) connection);
                    //}

                    System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);

                    context.Connections.Remove(connection);
                    context.SaveChanges();

                    UpdateChatRoomUsers();
                }
            }

            return base.OnDisconnected();
        }
        #endregion CONNECTION STATUS UPDATE

        #region CHATLOG HANDLING
        public static IList<ChatMessage> GetRecentChatLog(string roomId = DefaultRoomId)
        {
            //// TODO: Filter private/game/other messages.
            //ChatLog currentLog = (ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName];
            //if (currentLog.Messages.Count > 10)
            //{
            //    currentLog.Messages = currentLog.Messages.GetRange(currentLog.Messages.Count - 10, 10); //Where(m => (m.TimeSend - DateTime.Now) < new TimeSpan(0, 1, 0)).ToList();
            //}
            //return currentLog.Messages;
            using (var context = new MagicDbContext())
            {
                var chatRoom = context.ChatRooms.Find(roomId);
                return chatRoom.Log.Messages;
            }
        }

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

        private void AddMessageToChatLog(ChatMessage message, string roomId = DefaultRoomId)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = context.ChatRooms.Find(roomId);
                chatRoom.Log.Messages.Add(message);
                context.InsertOrUpdate(chatRoom);
            }

            // TODO: Possible issue occuring over period of 3 mins between message log saving. If message sent near midnight, the log of the day before may contain messages from the current day.
            // Suggested solution: Add new temporary message log or explicitly call log saving.

            //if (((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName]).DateCreated == message.TimeSend.Value.Date)
            //{
            // Synchronize adding message to ChatLog.
            //HttpContext.Current.ApplicationInstance.Context.Application.Lock();
            //((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName]).Messages.Add(message);
            //HttpContext.Current.ApplicationInstance.Context.Application.UnLock();
            //}
        }
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