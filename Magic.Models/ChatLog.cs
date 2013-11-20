using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatLog : AbstractExtensions
    {
        [Key]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public virtual List<ChatMessage> MessageLog { get; set; }

        // Constructor.
        public ChatLog()
        {
            DateCreated = DateTime.Today;
            MessageLog = new List<ChatMessage>();
        }

        public void AppendMessages(List<ChatMessage> tempLog)
        {
            MessageLog.AddRange(tempLog);
        }
    }
}