using Juza.Magic.Models.Extensions;
using Juza.Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatRoom
    {
        public const string DefaultRoomId = "default";

        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColorCode { get; set; }
        public bool IsGameRoom { get; set; }
        public bool IsPrivate { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual ICollection<ChatRoomUser> Users { get; set; }
        public virtual ICollection<ChatRoomConnection> Connections { get; set; }

        public ChatRoom()
        {
            Id = Guid.NewGuid().ToString();
            IsGameRoom = false;
            IsPrivate = false;
            Users = new List<ChatRoomUser>();
            Connections = new List<ChatRoomConnection>();
        }

        public void AddMessageToLog(ChatMessage message)
        {
            if (Log == null)
            {
                Log = new ChatLog
                {
                    Id = Id
                };
            }

            Log.Messages.Add(message);
        }

        public bool IsUserInRoom(int userId)
        {
            return Connections.Any(c => c.UserId == userId);
        }

        public bool IsUserAllowedToJoin(int userId)
        {
            return IsPrivate == false || Users.Any(u => u.UserId == userId);
        }

        public int ActiveUserCount()
        {
            return Connections.Distinct(new ChatRoomConnection_UserComparer()).Count();
        }

        public IList<ChatUserViewModel> GetUserList()
        {
            return IsPrivate ? Users.Select(u => new ChatUserViewModel(u.User)).ToList()
                : Connections.Distinct(new ChatRoomConnection_UserComparer()).Select(u => new ChatUserViewModel(u.User)).ToList();
        }

        public IList<ChatUserViewModel> GetActiveUserList()
        {
            var users = Connections.Select(c => c.User).Distinct();
            return users.Select(user => new ChatUserViewModel(user)).ToList();
        }

        public bool OnlySpecifiedUsersInRoom(IEnumerable<int> userIds)
        {
            var allowedUserIds = Users.Select(u => u.UserId);
            return !allowedUserIds.Except(userIds).Union(userIds.Except(allowedUserIds)).Any(); ;
        }
    }

    public class ChatRoomViewModel : IViewModel<ChatRoom>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColorCode { get; set; }
        public bool IsGameRoom { get; set; }
        public bool IsPrivate { get; set; }
        public IEnumerable<ChatUserViewModel> Users { get; set; }
        public ChatLogViewModel Log { get; set; }

        public ChatRoomViewModel()
        {
            IsGameRoom = false;
            IsPrivate = false;
            Users = new List<ChatUserViewModel>();
            Log = new ChatLogViewModel();
        }
        public ChatRoomViewModel(ChatRoom room)
        {
            Id = room.Id;
            Name = room.Name;
            IsGameRoom = room.IsGameRoom;
            IsPrivate = room.IsPrivate;
            TabColorCode = room.TabColorCode;
            Users = room.GetUserList();
            Log = (room.Log != null ? room.Log.ToViewModel<ChatLog, ChatLogViewModel>() : new ChatLogViewModel());
        }
    }
}