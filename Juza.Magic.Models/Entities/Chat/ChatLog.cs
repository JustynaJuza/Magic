using System;
using System.Collections.Generic;
using System.Linq;
using Juza.Magic.Models.Extensions;
using Juza.Magic.Models.Interfaces;

namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatLog : AbstractExtensions
    {
        public string Id { get; set; }
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

        public IList<ChatMessageViewModel> GetUserMessages(int userId)
        {
            return Messages.Select(m => new ChatMessageViewModel(m)
            {
                IsRead = m.Recipients.Any(r => r.RecipientId == userId) 
            
            }).ToList();
        }
    }

    public class ChatLogViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual IList<ChatMessageViewModel> Messages { get; set; }

        // Constructor.
        public ChatLogViewModel() {
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