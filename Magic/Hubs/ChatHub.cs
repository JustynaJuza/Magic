using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Hubs
{
    public class ChatHub : Hub
    {
        private static MagicDBContext context = new MagicDBContext();

        public void Send(string messageText)
        {
            string recipientName = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipientName.Length > 0)
            {
                // Get message text after username and following space.
                messageText = messageText.Substring(recipientName.Length+1);
                recipientName = recipientName.Substring(1);
            }

            var message = new ChatMessage()
            {
                Message = messageText,
                Sender = context.Users.Find(Context.User.Identity.GetUserId()),
                Recipient = context.Users.FirstOrDefault(u => u.UserName == recipientName),
                TimeSend = DateTime.Now
            };

            // Synchronize adding message to ChatLog.
            HttpContext.Current.ApplicationInstance.Context.Application.Lock();
            ((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application["GeneralChatLog"]).MessageLog.Add(message);
            HttpContext.Current.ApplicationInstance.Context.Application.UnLock();

            // Use callback method to update clients.
            if (message.Recipient == null)
            {
                Clients.All.addNewMessageToPage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
            }
            else
            {
                Clients.Caller.addNewMessageToPage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message, message.Recipient.UserName, message.Recipient.ColorCode);
                Clients.User(message.Recipient.Id).addNewMessageToPage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
            }
        }

        public static void UserActionBroadcast(string userId, bool joinedChat = true)
        {

            var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.ChatHub>();

            var message = new ChatMessage()
            {
                Sender = context.Users.Find(userId),
                TimeSend = DateTime.Now
            };

            if (joinedChat)
            {
                message.Message = " joined the conversation.";
            }
            else
            {
                message.Message = " left.";
            }

            // Synchronize adding message to ChatLog.
            HttpContext.Current.ApplicationInstance.Context.Application.Lock();
            ((ChatLog) HttpContext.Current.ApplicationInstance.Context.Application["GeneralChatLog"]).MessageLog.Add(message);
            HttpContext.Current.ApplicationInstance.Context.Application.UnLock();

            hubContext.Clients.All.addNewMessageToPage(message.TimeSend.Value.ToString("HH:mm:ss"), message.Sender.UserName, message.Sender.ColorCode, message.Message);
        }

        #region CHATLOG SAVE
        // This function is called by schedule from Global.asax and uses the static context.
        public static string SaveChatLogToDatabase(ChatLog currentLog)
        {
            ChatLog todayLog = context.ChatLogs.Find(currentLog.DateCreated);//context.Set<ChatLog>().AsNoTracking().FirstOrDefault(c => c.DateCreated == currentLog.DateCreated);
            if (todayLog != null)
            {
                todayLog.AppendMessages(currentLog);
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