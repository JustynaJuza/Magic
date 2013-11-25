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
    public class ChatHub : ConnectionHub
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

        public static void UserActionBroadcast(string userId, bool joinedChat = true)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.ChatHub>();

            var message = new ChatMessage()
            {
                Sender = context.Users.Find(userId),
                Message = joinedChat ? " joined the conversation." : " left."
            };

            hubContext.Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }

        #region GROUPS
        public static void ActivateGameChat(string userId, string roomName, bool joinedChat = true)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.ChatHub>();

            var message = new ChatMessage()
            {
                Sender = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId),
                Message = joinedChat ? " entered the game." : " left the game."
            };

            if (joinedChat)
            {
                foreach (var connection in message.Sender.Connections)
                {
                    hubContext.Groups.Add(connection.Id, roomName);
                }
            }
            else
            {
                foreach (var connection in message.Sender.Connections)
                {
                    hubContext.Groups.Remove(connection.Id, roomName);
                }
            }
            hubContext.Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }

        //public async Task JoinRoom(string connectionId, string roomName)
        //{
        //    await Groups.Add(connectionId, roomName);
        //}

        //public async Task LeaveRoom(string userId, string roomName)
        //{
        //    try
        //    {
        //        var foundUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);
        //        foreach (var connection in foundUser.Connections)
        //        {
        //            await Groups.Remove(Context.ConnectionId, roomName);
        //        }
        //        Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " left the game room.");
        //    }
        //    catch (Exception) { }
        //}
        #endregion GROUPS

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

        #region HELPERS
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