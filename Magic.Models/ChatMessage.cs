using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatMessage : AbstractExtensions
    {
        public int Id { get; set; }
        public DateTime? TimeRead { get; set; }
        public DateTime? TimeSend { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Recipient { get; set; }
        public string Message { get; set; }
    }

    public class ChatLog : AbstractExtensions
    {
        [Key]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }
        public virtual List<ChatMessage> MessageLog { get; set; }

        // Constructor.
        public ChatLog()
        {
            DateCreated = DateTime.Today;
            MessageLog = new List<ChatMessage>();
        }

        #region HELPERS
        public void AppendMessages(ChatLog tempLog)
        {
            MessageLog.AddRange(tempLog.MessageLog);
        }
        #endregion HELPERS
    }
}