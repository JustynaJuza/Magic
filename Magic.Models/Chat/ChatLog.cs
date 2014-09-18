using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Magic.Models.Interfaces;

namespace Magic.Models
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

        public IList<ChatMessageViewModel> GetUserMessages(string userId)
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
            Messages = log.GetUserMessages(null);
        }
        public ChatLogViewModel(ChatLog log, string userId) : this()
        {
            Messages = log.GetUserMessages(userId);
        }
    }
}