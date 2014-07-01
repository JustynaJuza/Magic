using Magic.Models.Helpers;
using System;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatMessage : AbstractExtensions
    {
        public int Id { get; set; }
        public bool IsUnread { get; set; }
        public DateTime? TimeSend { get; set; }
        public string Message { get; set; }
        public virtual string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Recipient { get; set; }
        public virtual ChatLog Log { get; set; }

        // Constructor.
        public ChatMessage()
        {
            TimeSend = DateTime.Now;
        }
        // Constructor with message.
        public ChatMessage(string messageText)
        {
            TimeSend = DateTime.Now;
            Message = messageText;
        }
    }

    public class ChatMessageViewModel : AbstractExtensions
    {
        public int Id { get; set; }
        public DateTime? TimeSend { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }

        // Constructor.
        public ChatMessageViewModel(ChatMessage message)
        {
            Id = message.Id;
            TimeSend = message.TimeSend;
            Message = message.Message;
            SenderName = message.Sender.UserName;
            if (message.Recipient != null)
            {
                RecipientName = message.Recipient.UserName;
            }
        }
    }
}