﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatMessage : AbstractToString
    {
        public int Id { get; set; }
        public DateTime? TimeRead { get; set; }
        public DateTime? TimeSend { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Recipient { get; set; }
        public string Message { get; set; }
    }

    public class ChatLog : AbstractToString
    {
        [Key]
        public DateTime DateCreated { get; set; }
        public virtual List<ChatMessage> MessageLog { get; set; }

        public ChatLog()
        {
            DateCreated = DateTime.Today;
        }
    }
}