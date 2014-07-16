using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatLog : AbstractExtensions
    {
        public string Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public virtual List<ChatMessage> Messages { get; set; }

        // Constructor.
        public ChatLog()
        {
            DateCreated = DateTime.Today;
            Messages = new List<ChatMessage>();
        }

        public ChatLog(string id) : this()
        {
            Id = id;
        }

        public void AppendMessages(List<ChatMessage> tempLog)
        {
            Messages.AddRange(tempLog);
        }
    }
}