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
        public const string DefaultRoomId = "00000000-0000-0000-0000-000000000000";
        private static MagicDbContext context = new MagicDbContext();

        #region CHAT INIT
        public void ListChatRooms() {
            Json.Encode(context.ChatRooms.Where(r => r.UserConnections.Any(c => c.User == Context.User)));
        }

        public void GetChatRoomUsers(string roomId = DefaultRoomId) {
            var room = context.ChatRooms.Find(roomId);

            var chatUsers = new List<ChatUserViewModel>();
            foreach (var connection in room.UserConnections){
                chatUsers.Add(new ChatUserViewModel(connection.User));
            }

            Json.Encode(chatUsers);
        }

        public void AddChatRoom() { 
            
        }
        #endregion CHAT INIT

        #region CHAT MESSAGE HANDLING
        public void Send(string messageText, string roomId = DefaultRoomId, string recipientName = "")
        {
            ApplicationUser recipient;
            var messageIsValid = ValidateMessage(messageText, recipientName, out recipient);

            if (messageIsValid)
            {
                var userId = Context.User.Identity.GetUserId();
                var message = new ChatMessage(messageText)
                {
                    Sender = context.Users.Find(userId),
                    Recipient = recipient,
                    Message = messageText
                };

                var chatRoom = context.ChatRooms.Find(roomId);
                if (recipient != null && chatRoom == null)
                {
                    // Create new chat room if no id was given for private conversation.
                    chatRoom = new ChatRoom() { Name = recipientName };
                }

                    // Add message to chatlog in correct room.
                    chatRoom.Log.Messages.Add(message);
                    context.Update(chatRoom);

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
                    message.Message = messageText.Substring(recipient.UserName.Length + 2);
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
            }
        }

        public static void UserStatusBroadcast(string userId, UserStatus status, string roomName = DefaultRoomId)
        {
            var message = new ChatMessage
            {
                Sender = context.Users.Find(userId),
                Message = UserStatusBroadcastMessage(status)
            };
            message.Sender.Status = status;
            context.Update(message.Sender, true);

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

        private string DecodeRecipient(string messageText, out ApplicationUser recipient)
        {
            var recipientName = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipientName.Length > 0)
            {
                recipientName = recipientName.Substring(1);
            }
            // Try to find recipient.
            recipient = context.Users.FirstOrDefault(u => u.UserName == recipientName);
            return recipientName;
        }
        #endregion CHAT MESSAGE HANDLING

        #region MANAGE CHAT & GAME GROUPS
        public void ToggleChatRoomsSubscription(ApplicationUserConnection connection, bool activate = true)
        {
            var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            
            if (activate)
            {
                var chatRooms = context.ChatRooms.Where(r => r.UserConnections.Any(c => c.User.Id == connection.User.Id));
                foreach(var chatRoom in chatRooms){
                    chatRoom.UserConnections.Add(connection);
                    context.Update(chatRoom);

                    chatHubContext.Groups.Add(connection.Id, chatRoom.Id);
                }
            }
            else
            {
                var chatRooms = context.ChatRooms.Where(r => r.UserConnections.Any(c => c.Id == connection.Id));
                foreach(var chatRoom in chatRooms){
                    chatRoom.UserConnections.Remove(connection);
                    context.Update(chatRoom);

                    chatHubContext.Groups.Remove(connection.Id, chatRoom.Id);
                }
            }

            //var chatUsers = chatRoom.UserConnections.Select(u => new ChatUserViewModel(u.User));

            //var foundChatUser = chatUsers.FirstOrDefault(u => u.UserName == user.UserName);
            //if (foundChatUser != null)
            //{
            //    chatUsers.Remove(foundChatUser);
            //}
            //else
            //{
            //    chatUsers.Add(new ChatUserViewModel(user));
            //}

            chatHubContext.Clients.All.updateChatRoomUser(connection.User.UserName, connection.User.ColorCode);
        }

        public async void ToggleGameChatSubscription(string gameId, bool activate)
        {
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();
            var foundUser = context.Users.Find(userId);
            var message = new ChatMessage
            {
                Sender = foundUser,
                Message = activate ? " entered the game." : " left the game."
            };

            if (activate)
            {
                // Set the current connection as game connection.
                var currentConnection = foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
                var gameConnection = new ApplicationUserGameConnection
                {
                        Id = currentConnection.Id,
                        User = foundUser,
                        //ChatRoom = context.ChatRooms.Find(gameId) ?? new ChatRoom { Id = gameId },
                        Game = context.Games.Find(gameId) ?? new Game { Id = gameId }
                    };

                foundUser.Connections.Remove(currentConnection);
                context.Update(foundUser, true);
                foundUser.GameConnections.Add(gameConnection);
                context.Update(foundUser, true);

                // Await to join the group on main connection so the joining user get's the info message.
                await GameHub.JoinGame(gameConnection.Id, gameId);

                // Subscribe all other chat connections.
                foreach (var connection in foundUser.Connections)
                {
                    Groups.Add(connection.Id, gameId);
                }
            }
            else
            {
                // Closing main connection to a game, remove chat subscribtion from all other connections.
                foreach (var connection in message.Sender.Connections)
                {
                    Groups.Remove(connection.Id, gameId);
                }
            }

            System.Diagnostics.Debug.WriteLine("Joining " + gameId);
            // Sent info message on joining and leaving group.
            await Clients.Group(gameId).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }
        #endregion MANAGE CHAT & GAME GROUPS

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();
            var foundUser = context.Users.Find(userId);

            if (foundUser != null)
            {
                var connection = new ApplicationUserConnection
                {
                    Id = Context.ConnectionId
                };
                //foundUser.Status = UserStatus.Online;
                foundUser.Connections.Add(connection);
                context.Update(foundUser);

                // Causes primary key conflict on connection :(
                ToggleChatRoomsSubscription(connection);

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
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();
            var foundUser = context.Users.Find(userId);

            if (foundUser != null)
            {
                if (foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId) == null)
                {
                    foundUser.Connections.Add(new ApplicationUserConnection
                    {
                        Id = Context.ConnectionId,
                        User = foundUser
                    });
                    context.Update(foundUser);
                }

                if (foundUser.Connections.Count == 1)
                {
                    // If this is the user's only connection broadcast a chat info.
                    UserStatusBroadcast(userId, UserStatus.Online);
                }
            }

            System.Diagnostics.Debug.WriteLine("Reconnected: " + Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            var connection = context.Connections.Find(Context.ConnectionId);

            if (connection != null)
            {
                if (connection.User.Connections.Count == 1)
                {
                    // If this is the user's last connection broadcast a chat info.
                    UserStatusBroadcast(connection.User.Id, UserStatus.Offline);
                }

                ToggleChatRoomsSubscription(connection, false);

                if (connection.GetType() == typeof(ApplicationUserGameConnection))
                {
                    //ToggleGameChatSubscription(connection.ChatRoomId, false);
                    //GameHub.DisplayUserLeft(connection.User.UserName, connection.ChatRoomId);
                    GameHub.LeaveGame((ApplicationUserGameConnection) connection);
                }


                System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);
                context.Delete(connection);
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
            var chatRoom = context.ChatRooms.Find(roomId);
            return chatRoom.Log.Messages;
        }

        // This function is called by schedule from Global.asax and uses the static context to save recent chat messages.
        public static string SaveChatLogToDatabase(ChatLog currentLog)
        {
            ChatLog todayLog = context.ChatLogs.Find(currentLog.DateCreated);//context.Set<ChatLog>().AsNoTracking().FirstOrDefault(c => c.DateCreated == currentLog.DateCreated);
            if (todayLog != null)
            {
                todayLog.AppendMessages(currentLog.Messages);
                return context.Update(todayLog, true);
            }

            // Create new log for Today.
            todayLog = new ChatLog
            {
                Messages = currentLog.Messages
            };
            return context.Create(todayLog);
        }

        private static void AddMessageToChatLog(ChatMessage message, string roomId = DefaultRoomId)
        {
            var chatRoom = context.ChatRooms.Find(roomId);
            chatRoom.Log.Messages.Add(message);
            context.Update(chatRoom);
            
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
    }
}