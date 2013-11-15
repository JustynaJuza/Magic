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
    public class ChatHub : Hub
    {
        private static MagicDBContext context = new MagicDBContext();

        public void Send(string messageText, string roomName = "")
        //public void Send(string messageText)
        {
            var recipient = DecodeRecipient(messageText);
            if (recipient != null && recipient.Status == UserStatus.Offline)
            {
                // Valid recipient but is offline, alert sender.
                Clients.Caller.addMessage(DateTime.Now.ToString("HH:mm:ss"), "ServerInfo", "#FFFFFF", "is currently offline and unable to receive messages.", recipient.UserName, recipient.ColorCode);
            }
            else
            {
                var message = new ChatMessage(messageText)
                {
                    Sender = context.Users.Find(Context.User.Identity.GetUserId()),
                    Recipient = recipient
                };

                AddMessageToChatLog(message, "GeneralChatLog");

                // Use callback method to update clients.
                if (message.Recipient == null)
                {
                    //if (roomName != "")
                    //{
                    //    Clients.Group(roomName).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                    //}
                    //else
                    //{
                        Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                    //}
                }
                else
                {
                    Clients.Caller.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message, message.Recipient.UserName, message.Recipient.ColorCode);
                    Clients.User(message.Recipient.Id).addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
                }
            }
        }

        #region GROUPS
        public async Task JoinRoom(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);
            Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " joined.");
        }

        public async Task LeaveRoom(string roomName)
        {
            try
            {
                await Groups.Remove(Context.ConnectionId, roomName);
                Clients.Group(roomName).addChatMessage(Context.User.Identity.Name + " left.");
            }
            catch (Exception) { }
        }
        #endregion GROUPS

        public static void UserActionBroadcast(string userId, bool joinedChat = true)
        {

            var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.ChatHub>();

            var message = new ChatMessage()
            {
                Sender = context.Users.Find(userId),
            };

            if (joinedChat)
            {
                message.Message = " joined the conversation.";
            }
            else
            {
                message.Message = " left.";
            }

            AddMessageToChatLog(message, "GeneralChatLog");
            hubContext.Clients.All.addMessage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }

        #region HELPERS

        public ApplicationUser DecodeRecipient(string messageText)
        {
            string recipientName = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipientName.Length > 0)
            {
                // Get message text after username and following space.
                messageText = messageText.Substring(recipientName.Length + 1);
                recipientName = recipientName.Substring(1);
            }
            return context.Users.FirstOrDefault(u => u.UserName == recipientName);
        }

        public static void AddMessageToChatLog(ChatMessage message, string logName)
        {
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