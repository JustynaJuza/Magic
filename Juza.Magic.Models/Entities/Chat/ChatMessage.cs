﻿using System;
using System.Collections.Generic;
using Juza.Magic.Models.Extensions;
using Juza.Magic.Models.Interfaces;

namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatMessageNotification : AbstractExtensions
    {
        public bool IsRead { get; set; }

        public int MessageId { get; set; }
        public string LogId { get; set; }
        public int RecipientId { get; set; }
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
        public DateTime TimeSent { get; set; }
        public string Message { get; set; }
        public virtual User Sender { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<ChatMessageNotification> Recipients { get; set; }

        // Constructor.
        public ChatMessage()
        {
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
        public DateTime? TimeSent { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }

        // Constructor.
        public ChatMessageViewModel() { }
        public ChatMessageViewModel(ChatMessage message)
        {
            TimeSent = message.TimeSent;
            Message = message.Message;
            SenderName = message.Sender.UserName;
        }
    }
}