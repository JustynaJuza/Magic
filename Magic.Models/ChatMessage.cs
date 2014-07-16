using Magic.Models.Helpers;
using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;

namespace Magic.Models
{
    public class Recipient_ChatMessageStatus : AbstractExtensions
    {
        public int MessageId { get; set; }
        public string RecipientId { get; set; }
        public bool IsUnread { get; set; }
        public virtual ChatMessage Message{ get; set; }
        public virtual ApplicationUser Recipient { get; set; }
    }

    public class ChatMessage : AbstractExtensions
    {
        public int Id { get; set; }
        public string LogId { get; set; }
        public string SenderId { get; set; }
        public DateTime? TimeSend { get; set; }
        public string Message { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<Recipient_ChatMessageStatus> Recipients { get; set; }

        // Constructor.
        public ChatMessage()
        {
            TimeSend = DateTime.Now;
            Recipients = new List<Recipient_ChatMessageStatus>();
        }

        // Constructor with message.
        public ChatMessage(string messageText) : this()
        {
            Message = messageText;
        }
    }

    //public class ChatMessageViewModel : AbstractExtensions
    //{
    //    public int Id { get; set; }
    //    public DateTime? TimeSend { get; set; }
    //    public string Message { get; set; }
    //    public string SenderName { get; set; }
    //    public string RecipientName { get; set; }

    //    // Constructor.
    //    public ChatMessageViewModel(ChatMessage message)
    //    {
    //        Id = message.Id;
    //        TimeSend = message.TimeSend;
    //        Message = message.Message;
    //        SenderName = message.Sender.UserName;
    //        if (message.Recipient != null)
    //        {
    //            RecipientName = message.Recipient.UserName;
    //        }
    //    }
    //}
}