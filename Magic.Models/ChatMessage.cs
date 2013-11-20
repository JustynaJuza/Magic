using Magic.Models.Helpers;
using System;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatMessage : AbstractExtensions
    {
        public int Id { get; set; }
        public DateTime? TimeSend { get; set; }
        public string Message { get; set; }
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
            Message = messageText;
            TimeSend = DateTime.Now;
        }
    }
}