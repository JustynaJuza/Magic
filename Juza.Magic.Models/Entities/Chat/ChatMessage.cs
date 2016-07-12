using Juza.Magic.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatMessageNotification
    {
        public bool IsRead { get; set; }

        public int MessageId { get; set; }
        public string LogId { get; set; }
        public int RecipientId { get; set; }
        public ChatMessage Message { get; set; }
        public User Recipient { get; set; }

        public ChatMessageNotification()
        {
            IsRead = false;
        }
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public string LogId { get; set; }
        public int SenderId { get; set; }
        public DateTime TimeSent { get; set; }
        public string Message { get; set; }
        public virtual User Sender { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual ICollection<ChatMessageNotification> RecipientNotifications { get; set; }

        public ChatMessage()
        {
            RecipientNotifications = new List<ChatMessageNotification>();
        }
    }

    public class ChatMessageViewModel : IViewModel<ChatMessage>
    {
        public DateTime TimeSent { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }

        public ChatMessageViewModel() { }
        public ChatMessageViewModel(ChatMessage message)
        {
            TimeSent = message.TimeSent;
            Message = message.Message;
            SenderName = message.Sender.UserName;
        }
    }
}