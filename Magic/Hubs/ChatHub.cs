using System;
using System.Collections.Generic;
using System.Linq;
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
        public static IList<ChatRoom> GetUserChatRooms(string userId)
        {
            using (var context = new MagicDbContext())
            {
                //var userId = Context.User.Identity.GetUserId();
                var chatRooms = context.ChatRoom_Connections.Select(rc => rc.ChatRoom).Distinct().Where(r => r.Connections.Any(c => c.UserId == userId) && r.Id != DefaultRoomId); //.OrderByDescending(r => r.Name == DefaultRoomId);
                return chatRooms.ToList();
                //Clients.Caller.loadActiveChatRooms(Json.Encode(chatRooms));
            }
        }

        public string GetChatRoom(string[] recipientNames)
        {
            using (var context = new MagicDbContext())
            {
                var recipients = new List<ApplicationUser>();
                foreach (var userName in recipientNames.Distinct())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserName == userName);
                    if (user == null) {
                        Clients.Caller.addMessage(DefaultRoomId, DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "User " + userName + " was not found!");
                    }
                    else
                    {
                        recipients.Add(user);
                    }
                }

                var recipientIds = recipients.Select(r => r.Id);
                //var recipientColors = recipients.Select(r => r.ColorCode).ToList();

                var chatRoom = context.ChatRooms.Where(r => r.IsPrivate).ToList().FirstOrDefault(r => r.OnlySpecifiedUsersInRoom(recipientIds));

                if (chatRoom != null)
                {
                    foreach (var user in recipients)
                    {
                        SubscribeActiveConnections(chatRoom.Id, user.Id);
                    }
                    return chatRoom.Id;
                    //Clients.Caller.loadChatRoom(chatRoom.Id, chatRoom.Name, recipientColors, recipientNames, Json.Encode(chatRoom.Log.GetUserMessages(userId)));
                    //var userId = Context.User.Identity.GetUserId();
                    //return ViewRenderer.RenderPartialView("~/Views/Shared/_ChatRoomPartial.cshtml", (ChatRoomViewModel)chatRoom.GetViewModel(userId));
                }

                return String.Empty;
            }
        }

        public void CreateChatRoom(string[] recipientNames, string roomId = null)
        {
            using (var context = new MagicDbContext())
            {
                var recipients = new List<ApplicationUser>();
                foreach (var userName in recipientNames.Distinct())
                {
                    recipients.Add(context.Users.FirstOrDefault(u => u.UserName == userName));
                }

                var chatRoom = new ChatRoom()
                {
                    IsPrivate = true,
                    Users = recipients
                };
                if (roomId != null) {
                    chatRoom.Id = roomId;
                }
                context.Insert(chatRoom);

                foreach (var user in recipients)
                {
                    SubscribeActiveConnections(chatRoom.Id, user.Id);
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
                var chatRoomConnection = new ChatRoom_ApplicationUserConnection
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

        public void UpdateChatRoomUsers(string roomId = DefaultRoomId, bool callerOnly = false)
        {
            using (var context = new MagicDbContext())
            {
                var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(c => c.User)).First(r => r.Id == roomId);
                
                var chatUsers = (roomId == DefaultRoomId ? chatRoom.GetActiveUserList() : chatRoom.GetUserList());

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

                var chatRoom = context.ChatRooms.Find(roomId); // ?? CreateChatRoom(recipientNames, roomId);
                
                foreach (var recipient in chatRoom.Users.Except(new ApplicationUser[] { sender }))
                {
                    message.Recipients.Add(new Recipient_ChatMessageStatus()
                    {
                        Recipient = recipient
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
        }

        public void SubscribeActiveConnections(string roomId, string userId)
        {
            using (var context = new MagicDbContext())
            {
                var activeConnectionIds = context.Connections.Where(c => c.UserId == userId).Select(c => c.Id);
                var subscribedConnectionIds = context.ChatRoom_Connections.Where(crc => crc.UserId == userId && crc.ChatRoomId == roomId).Select(crc => crc.ConnectionId);
                var unsubscribedConnectionIds = activeConnectionIds.Except(subscribedConnectionIds);

                foreach (var connectionId in unsubscribedConnectionIds)
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