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
        public bool IsGameRoom { get; set; }
        public bool IsPrivate { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<ChatRoomUser> Users { get; set; }
        public virtual IList<ChatRoomUserConnection> Connections { get; set; }

        public ChatRoom() { }

        public ChatRoom(string roomId = null, bool isGameRoom = false, bool isPrivate = false)
        {
            Id = roomId ?? Guid.NewGuid().ToString();
            IsGameRoom = isGameRoom;
            IsPrivate = isPrivate;
            Users = new List<ChatRoomUser>();
            Connections = new List<ChatRoomUserConnection>();
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
            return Users.Select(u => u.User).Select(user => new ChatUserViewModel(user)).ToList();
        }

        public IList<ChatUserViewModel> GetActiveUserList()
        {
            var users = Connections.Select(c => c.User).Distinct().ToList();

            //if (!string.IsNullOrWhiteSpace(exceptUserId))
            //{
            //    users.Remove(users.First(u => u.Id == exceptUserId));
            //}

            return users.Select(user => new ChatUserViewModel(user)).ToList();
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