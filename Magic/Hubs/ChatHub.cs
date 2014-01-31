using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        private class ChatUserViewModel {
            public string UserName { get; set; }
            public string ColorCode { get; set; }

            public ChatUserViewModel(ApplicationUser user)
            {
                UserName = user.UserName;
                ColorCode = user.ColorCode;
            }
        };

        private static MagicDBContext context = new MagicDBContext();
        private static List<ChatUserViewModel> chatUsers = new List<ChatUserViewModel>();

        #region CHAT MESSAGE HANDLING
        public void Send(string messageText, string roomName = "")
        {
            ApplicationUser recipient;
            var messageIsValid = ValidateMessage(messageText, out recipient);

            if (messageIsValid)
            {
                var userId = Context.User.Identity.GetUserId();
                var message = new ChatMessage(messageText)
                {
                    Sender = context.Users.Find(userId),
                    Message = messageText
                };

                // Depending on settings, use callback method to update clients.
                if (recipient == null)
                {
                    if (roomName.Length > 0)
                    {
                        AddMessageToChatLog(message, roomName);
                        Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                    }
                    else
                    {
                        AddMessageToChatLog(message);
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

        public static void UserStatusBroadcast(string userId, UserStatus status, string roomName = "")
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.ChatHub>();

            var message = new ChatMessage()
            {
                Sender = context.Users.Find(userId),
                Message = UserStatusBroadcastMessage(status)
            };
            message.Sender.Status = status;
            context.Update(message.Sender, true);

            if (roomName.Length > 0)
            {
                hubContext.Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
            }
            else
            {
                hubContext.Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
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

        private bool ValidateMessage(string messageText, out ApplicationUser recipient)
        {
            if (messageText.Length > 0)
            {
                var recipientName = DecodeRecipient(messageText, out recipient);

                if (recipientName.Length > 0 && recipient == null)
                {
                    // Recipient included but invalid, alert sender.
                    Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
                        "- no such user found, have you misspelled the name?", recipientName, "#696969");
                    return false;
                }
                else
                {
                    if (recipient != null)
                    {
                        // Valid recipient found.
                        if (recipient.Status == UserStatus.Offline)
                        {
                            // Valid recipient but is offline, alert sender.
                            Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
                                "is currently offline and unable to receive messages.", recipient.UserName, recipient.ColorCode);
                            return false;
                        }
                        if (messageText.Length < recipient.UserName.Length + 2)
                        {
                            // Valid recipient but no message appended, alert sender.
                            Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000",
                                "is online but no message was included.", recipient.UserName, recipient.ColorCode);
                            return false;
                        }
                    }

                    // Everything OK.
                    return true;
                }
            }

            // No message text at all.
            recipient = null;
            return false;
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

        //private string DecodeGroup(string messageText)
        //{
        //    string groupName = System.Text.RegularExpressions.Regex.Match(messageText, "^$([a-zA-Z]*").Value;
        //    if (groupName.Length > 0)
        //    {
        //        switch (groupName)
        //        {
        //            case "$Game":
        //                return "game";
        //        }
        //    }
        //    return null;
        //}
        #endregion CHAT MESSAGE HANDLING

        #region MANAGE CHAT & GAME GROUPS
        public void ToggleChatSubscription(ApplicationUser user)
        {
            var chatUserListEntry = new ChatUserViewModel(user);
            if (chatUsers.FirstOrDefault(u => u.UserName == chatUserListEntry.UserName) == null)
            {
                chatUsers.Add(chatUserListEntry);
            }
            else
            {
                chatUsers.Remove(chatUserListEntry);
            }

            Clients.All.updateChatUsers(Json.Encode(chatUsers));
        }

        public async void ToggleGameSubscription(string gameId, bool activate)
        {
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();
            var foundUser = context.Users.Find(userId);
            var message = new ChatMessage()
            {
                Sender = foundUser,
                Message = activate ? " entered the game." : " left the game."
            };

            if (activate)
            {
                // Set the connection as main connection.
                var gameConnection = foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
                gameConnection.GameId = gameId;
                context.Update(gameConnection, true);

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

            // Sent info message on joining and leaving group.
            Clients.Group(gameId).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }
        #endregion MANAGE CHAT & GAME GROUPS

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();
            var foundUser = context.Users.Find(userId);

            foundUser.Status = UserStatus.Online;
            foundUser.Connections.Add(new ApplicationUserConnection()
            {
                Id = Context.ConnectionId,
                User = foundUser
            });
            context.Update(foundUser);

            if (foundUser.Connections.Count == 1)
            {
                // If this is the user's only connection broadcast a chat info.
                ChatHub.UserStatusBroadcast(userId, UserStatus.Online);
            }

            ToggleChatSubscription(foundUser);
            System.Diagnostics.Debug.WriteLine("Connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            System.Diagnostics.Debug.WriteLine("Reconnected: " + Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            var connection = context.Connections.Find(Context.ConnectionId);
            if (connection != null && connection.GameId != null)
            {
                ToggleGameSubscription(connection.GameId, false);
                GameHub.DisplayUserLeft(connection);
                GameHub.LeaveGame(connection);
            }

            if (connection.User.Connections.Count == 1)
            {
                // If this is the user's last connection broadcast a chat info.
                ChatHub.UserStatusBroadcast(connection.User.Id, UserStatus.Offline);
            }

            ToggleChatSubscription(connection.User);
            System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);
            context.Delete(connection, true);

            return base.OnDisconnected();
        }
        #endregion CONNECTION STATUS UPDATE

        #region CHATLOG HANDLING
        public static IList<ChatMessage> GetRecentChatLog(string logName = "GeneralChatLog")
        {
            // TODO: Filter private/game/other messages.
            ChatLog currentLog = (ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName];
            if (currentLog.MessageLog.Count > 10)
            {
                currentLog.MessageLog = currentLog.MessageLog.GetRange(currentLog.MessageLog.Count - 10, 10); //Where(m => (m.TimeSend - DateTime.Now) < new TimeSpan(0, 1, 0)).ToList();
            }
            return currentLog.MessageLog;
        }

        // This function is called by schedule from Global.asax and uses the static context to save recent chat messages.
        public static string SaveChatLogToDatabase(ChatLog currentLog)
        {
            ChatLog todayLog = context.ChatLogs.Find(currentLog.DateCreated);//context.Set<ChatLog>().AsNoTracking().FirstOrDefault(c => c.DateCreated == currentLog.DateCreated);
            if (todayLog != null)
            {
                todayLog.AppendMessages(currentLog.MessageLog);
                return context.Update(todayLog, true);
            }
            else
            {
                // Create new log for Today.
                todayLog = new ChatLog()
                {
                    MessageLog = currentLog.MessageLog
                };
                return context.Create(todayLog);
            }
        }

        private static void AddMessageToChatLog(ChatMessage message, string logName = "GeneralChatLog")
        {
            // TODO: Possible issue occuring over period of 3 mins between message log saving. If message sent near midnight, the log of the day before may contain messages from the current day.
            // Suggested solution: Add new temporary message log or explicitly call log saving.

            //if (((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName]).DateCreated == message.TimeSend.Value.Date)
            //{
            // Synchronize adding message to ChatLog.
            HttpContext.Current.ApplicationInstance.Context.Application.Lock();
            ((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName]).MessageLog.Add(message);
            HttpContext.Current.ApplicationInstance.Context.Application.UnLock();
            //}
        }
        #endregion CHATLOG HANDLING
    }
}