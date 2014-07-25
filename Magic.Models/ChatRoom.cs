using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Magic.Models.DataContext;
using Magic.Models.Helpers;
using Magic.Models.Interfaces;

namespace Magic.Models
{
    public class ChatRoom : AbstractExtensions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColorCode { get; set; }
        public bool IsPrivate { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<ChatRoom_ApplicationUser> Users { get; set; }
        public virtual IList<ChatRoom_ApplicationUserConnection> Connections { get; set; }

        public ChatRoom()
        {
            Id = Guid.NewGuid().ToString();
            IsPrivate = false;
            Users = new List<ChatRoom_ApplicationUser>();
            Connections = new List<ChatRoom_ApplicationUserConnection>();
        }

        public ChatRoom(bool isPrivate)
            : this()
        {
            IsPrivate = isPrivate;
        }

        public void AddMessageToLog(ChatMessage message)
        {
            if (Log == null)
            {
                Log = new ChatLog(Id);
            }
            Log.Messages.Add(message);
        }

        public bool UserIsInRoom(string userId)
        {
            return Connections.Any(c => c.Connection.UserId == userId);
        }

        public int ActiveUserCount()
        {
            return Connections.Distinct(new ChatRoom_ApplicationUserConnection_UserComparer()).Count();
        }

        public IList<ChatUserViewModel> GetUserList()
        {
                var chatUsers = new List<ChatUserViewModel>();
                foreach (var user in Users.Select(u => u.User))
                {
                    chatUsers.Add(new ChatUserViewModel(user));
                }
                return chatUsers;
        }

        public IList<ChatUserViewModel> GetActiveUserList()
        {
            var chatUsers = new List<ChatUserViewModel>();
            foreach (var user in Connections.Select(c => c.User).Distinct())
            {
                chatUsers.Add(new ChatUserViewModel(user));
            }

            return chatUsers;
        }

        public bool OnlySpecifiedUsersInRoom(IEnumerable<string> userIds)
        {
            var allowedUserIds = Users.Select(u => u.UserId);
            return !allowedUserIds.Except(userIds).Union(userIds.Except(allowedUserIds)).Any(); ;
        }
    }

    public class ChatRoomViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColorCode { get; set; }
        public bool IsPrivate { get; set; }
        public IList<ChatUserViewModel> Users { get; set; }
        public ChatLogViewModel Log { get; set; }

        public ChatRoomViewModel() {
            Users = new List<ChatUserViewModel>();
            Log = new ChatLogViewModel();
        }
        public ChatRoomViewModel(ChatRoom room) {
            Id = room.Id;
            Name = room.Name;
            IsPrivate = room.IsPrivate;
            TabColorCode = room.TabColorCode;
            Users = room.GetUserList();
            Log = (room.Log != null ? (ChatLogViewModel) room.Log.GetViewModel() : new ChatLogViewModel());
        }
        public ChatRoomViewModel(ChatRoom room, string userId) : this(room)
        {
            Log = (room.Log != null ? (ChatLogViewModel)room.Log.GetViewModel(userId) : new ChatLogViewModel());
        }
    }
}