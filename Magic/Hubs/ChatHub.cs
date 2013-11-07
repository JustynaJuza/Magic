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
        MagicDBContext context = new MagicDBContext();

        public void Send(string messageText)
        {
            string recipient = System.Text.RegularExpressions.Regex.Match(messageText, "^@([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)").Value;
            if (recipient.Length > 0) recipient = recipient.Substring(1);

            ChatMessage message = new ChatMessage()
            {
                Message = messageText,
                Sender = context.Users.Find(Context.User.Identity.GetUserId()),
                Recipient = context.Users.FirstOrDefault(u => u.UserName == recipient),
                TimeSend = DateTime.Now
            };

            //// Update chatlog.
            //ChatLog currentLog = context.ChatLogs.Find(message.TimeSend.Value.Date);
            //if (currentLog != null)
            //{
            //    currentLog.MessageLog.Add(message);
            //    context.Update(currentLog);
            //    System.Diagnostics.Debug.WriteLine(currentLog.ToString());
            //}
            //else
            //{
            //    // Create new log for Today
            //    currentLog = new ChatLog();
            //    currentLog.MessageLog.Add(message);
            //    context.Create(currentLog);
            //}

            if (message.Recipient == null)
            {
                // Use callback method to update clients.
                Clients.All.addNewMessageToPage(message.TimeSend.Value.ToString("hh:mm:ss"), message.Sender.UserName, message.Message);
            }
            else
            {
                Clients.Caller.addNewMessageToPage(message.TimeSend.Value.ToString("hh:mm:ss"), message.Sender.UserName, message.Message);
                Clients.User(message.Recipient.Id).addNewMessageToPage(message.TimeSend.Value.ToString("hh:mm:ss"), message.Sender.UserName, message.Message);
            }
        }
    }
}