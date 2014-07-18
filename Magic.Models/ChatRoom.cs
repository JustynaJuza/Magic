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
        //public string ChatLogId { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public string TabColorCode { get; set; }
        public IList<string> AllowedUserIds { get; set; }
        public IList<string> TabColorCodes { get; set; }
        public IList<string> UserNames { get; set; }
        public virtual ChatLog Log { get; set; }
        //public virtual IList<ApplicationUser> Users { get; set; }
        public virtual IList<ChatRoom_ApplicationUserConnection> Connections { get; set; }

        public ChatRoom()
        {
            Id = Guid.NewGuid().ToString();
            IsPrivate = false;
            TabColorCode = String.Empty.AssignRandomColorCode();
            AllowedUserIds = new List<string>();
            TabColorCodes = new List<string>();
            UserNames = new List<string>();
            //Users = new List<ApplicationUser>();
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
                foreach (var user in Connections.Select(c => c.User).Distinct())
                {
                    chatUsers.Add(new ChatUserViewModel(user));
                }
                return chatUsers;
        }

        public bool OnlySpecifiedUsersInRoom(IEnumerable<string> userIds)
        {
            return AllowedUserIds.All(allowedId => userIds.Any(id => id == allowedId));
        }
    }

    public class ChatRoomViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public IList<string> TabColorCodes { get; set; }
        public IList<ChatUserViewModel> Users { get; set; }
        public ChatLogViewModel Log { get; set; }

        public ChatRoomViewModel() { }
        public ChatRoomViewModel(ChatRoom room) {
            Id = room.Id;
            Name = room.Name;
            IsPrivate = room.IsPrivate;
            TabColorCodes = room.TabColorCodes;
            Users = room.GetUserList();
            Log = (ChatLogViewModel) room.Log.GetViewModel();
        }
        public ChatRoomViewModel(ChatRoom room, string userId) : this()
        {
            Log = (ChatLogViewModel)room.Log.GetViewModel(userId);
        }
    }
}