using Magic.Models.Helpers;
using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using Magic.Models.Interfaces;

namespace Magic.Models
{
    public class ChatMessageNotification : AbstractExtensions
    {
        public bool IsRead { get; set; }

        public int MessageId { get; set; }
        public string LogId { get; set; }
        public string RecipientId { get; set; }
        public ChatMessage Message { get; set; }
        public User Recipient { get; set; }

        public ChatMessageNotification() {
            IsRead = false;
        }
        public ChatMessageNotification(bool isRead)
        {
            IsRead = isRead;
        }
    }

    public class ChatMessage : AbstractExtensions
    {
        public int Id { get; set; }
        public string LogId { get; set; }
        public string SenderId { get; set; }
        public DateTime? TimeSend { get; set; }
        public string Message { get; set; }
        public virtual User Sender { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<ChatMessageNotification> Recipients { get; set; }

        // Constructor.
        public ChatMessage()
        {
            TimeSend = DateTime.Now;
            Recipients = new List<ChatMessageNotification>();
        }

        // Constructor with message.
        public ChatMessage(string messageText) : this()
        {
            Message = messageText;
        }
    }

    public class ChatMessageViewModel : AbstractExtensions, IViewModel
    {
        public DateTime? TimeSend { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }

        // Constructor.
        public ChatMessageViewModel() { }
        public ChatMessageViewModel(ChatMessage message)
        {
            TimeSend = message.TimeSend;
            Message = message.Message;
            SenderName = message.Sender.UserName;
        }
    }
}