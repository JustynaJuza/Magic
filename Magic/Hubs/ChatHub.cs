using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Magic.Models;
using Magic.Models.DataContext;
using System.Threading.Tasks;

namespace Magic.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static MagicDBContext context = new MagicDBContext();

        public void Send(string messageText, string roomName = "")
        {
            var foundRecipient = DecodeRecipient(messageText);
            if (foundRecipient != null && foundRecipient.GetType() == typeof(string))
            {
                // Recipient included but invalid, alert sender.
                Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "- no such user found, have you misspelled the name?", foundRecipient, "#696969");
            }
            else
            {
                // Valid recipient found.
                var recipient = (ApplicationUser) foundRecipient;
                if (recipient != null)
                {
                    if (recipient.Status == UserStatus.Offline)
                    {
                        // Valid recipient but is offline, alert sender.
                        Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "is currently offline and unable to receive messages.", recipient.UserName, recipient.ColorCode);
                        return;
                    }
                    if (messageText.Length < recipient.UserName.Length + 2)
                    {
                        // Valid recipient but no message appended, alert sender.
                        Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#000000", "is online but no message was included.", recipient.UserName, recipient.ColorCode);
                        return;
                    }
                    // Get message text after username and following space.
                    messageText = messageText.Substring(recipient.UserName.Length + 2);
                }

                var userId = Context.User.Identity.GetUserId();
                var message = new ChatMessage(messageText)
                {
                    Sender = context.Users.Find(userId),
                    Recipient = recipient
                };

                // Use callback method to update clients.
                if (recipient == null)
                {
                    if (roomName != "")
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
                    foreach (var connection in message.Recipient.Connections) //.Where(c => c.Connected == true))
                    {
                        Clients.Client(connection.Id).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message, message.Recipient.UserName, message.Recipient.ColorCode);
                    }
                    foreach (var connection in message.Sender.Connections)
                    {
                        Clients.Client(connection.Id).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message, message.Recipient.UserName, message.Recipient.ColorCode);
                    }
                }
            }
        }

        #region GROUPS
        public void ActivateGameChat(string roomName = "")
        {
            bool joinedChat = roomName != "";

            var userId = Context.User.Identity.GetUserId();
            var foundUser = context.Users.Find(userId);
            var message = new ChatMessage()
            {
                Sender = foundUser,
                Message = joinedChat ? " entered the game." : " left the game."
            };

            if (joinedChat)
            {
                //var gameConnection = foundUser.Connections.FirstOrDefault(c => c.GameId == roomName);
                //if (gameConnection != null)
                //{
                //    // Check if game connection already exist, i.e. page was refreshed.
                //    //gameConnection.Id = Context.ConnectionId;
                //}
                //else
                //{
                // Add the connection as main connection and subscribe all other connections.
                var gameConnection = foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
                gameConnection.GameId = roomName;
                //}
                context.Update(gameConnection);

                foreach (var connection in foundUser.Connections)
                {
                    Groups.Add(connection.Id, roomName);
                }
            }
            else
            {
                // If closing main connection to a game remove subscribtion from all other connections.
                var gameConnection = foundUser.Connections.FirstOrDefault(c => c.Id == Context.ConnectionId);
                if (gameConnection != null)
                {
                    roomName = gameConnection.GameId;
                    // Remove all the connection subscriptions for this game.
                    foreach (var connection in message.Sender.Connections)
                    {
                        Groups.Remove(connection.Id, roomName);
                    }

                    System.Diagnostics.Debug.WriteLine("Removed all for " + gameConnection.Id);
                }
            }

            Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }
        #endregion GROUPS

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

            if (roomName != "")
            {
                hubContext.Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
            }
            else
            {
                hubContext.Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
            }
        }

        public static IList<ChatMessage> GetRecentChatLog(string logName = "GeneralChatLog")
        {
            // TODO: Filter private messages.
            ChatLog currentLog = (ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName];
            if (currentLog.MessageLog.Count > 10)
            {
                currentLog.MessageLog = currentLog.MessageLog.GetRange(currentLog.MessageLog.Count - 10, 10); //Where(m => (m.TimeSend - DateTime.Now) < new TimeSpan(0, 1, 0)).ToList();
            }
            return currentLog.MessageLog;
        }

        #region CONNECTION STATUS UPDATE
        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();
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
                ActivateGameChat();
                GameHub.PlayerLeft(connection);
            }

            if (connection.User.Connections.Count == 1)
            {
                // If this is the user's last connection broadcast a chat info.
                ChatHub.UserStatusBroadcast(connection.User.Id, UserStatus.Offline);
            }

            System.Diagnostics.Debug.WriteLine("Disconnected: " + connection.Id);
            context.Delete(connection, true);

            return base.OnDisconnected();
        }
        #endregion CONNECTION STATUS UPDATE

        #region HELPERS
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

        private object DecodeRecipient(string messageText)
        {
            string recipientName = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipientName.Length > 0)
            {
                recipientName = recipientName.Substring(1);
                var recipient = context.Users.FirstOrDefault(u => u.UserName == recipientName);
                if (recipient == null)
                {
                    return recipientName;
                }
                else
                {
                    return recipient;
                }
            }
            // No recipient at all.
            return null;
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

        private static void AddMessageToChatLog(ChatMessage message, string logName = "GeneralChatLog")
        {
            // TODO: Possible issue occuring over periof of 3 mins between message log saving. If message sent near midnight, the log of the day before may contain messages from the current day.
            // Suggested solution: Add new temporary message log or explicitly call log saving.

            //if (((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName]).DateCreated == message.TimeSend.Value.Date)
            //{
            // Synchronize adding message to ChatLog.
            HttpContext.Current.ApplicationInstance.Context.Application.Lock();
            ((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application[logName]).MessageLog.Add(message);
            HttpContext.Current.ApplicationInstance.Context.Application.UnLock();
            //}
        }
        #endregion HELPERS

        #region CHATLOG SAVE
        // This function is called by schedule from Global.asax and uses the static context.
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
        #endregion CHATLOG SAVE
    }
}