using Juza.Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatLog
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }

        public ChatLog()
        {
            DateCreated = DateTime.Today;
            Messages = new List<ChatMessage>();
        }

        public IEnumerable<ChatMessageViewModel> GetUserMessages(int userId)
        {
            return Messages.Select(m => new ChatMessageViewModel(m)
            {
                IsRead = m.RecipientNotifications.Any(r => r.RecipientId == userId)

            }).ToList();
        }
    }

    public class ChatLogViewModel : IViewModel<ChatLog>
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<ChatMessageViewModel> Messages { get; set; }

        // Constructor.
        public ChatLogViewModel()
        {
            Messages = new List<ChatMessageViewModel>();
        }
        public ChatLogViewModel(ChatLog log) : this()
        {
            Id = log.Id;
            DateCreated = log.DateCreated;
            Messages = log.GetUserMessages(default(int));
        }
        public ChatLogViewModel(ChatLog log, int userId) : this()
        {
            Messages = log.GetUserMessages(userId);
        }
    }
}