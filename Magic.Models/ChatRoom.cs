using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Magic.Models.Helpers;

namespace Magic.Models
{
    public class ChatRoom
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public string TabColorCode { get; set; }
        public virtual IList<string> TabColorCodes { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<string> AllowedUserIds{ get; set; }
        public virtual IList<ChatRoom_ApplicationUserConnection> Connections { get; set; }

        public ChatRoom() {
            Id = Guid.NewGuid().ToString();
            IsPrivate = false;
            TabColorCode = String.Empty.AssignRandomColorCode();
            Log = new ChatLog();
            AllowedUserIds = new List<string>();
            TabColorCodes = new List<string>();
            Connections = new List<ChatRoom_ApplicationUserConnection>();
        }

        public ChatRoom(bool isPrivate) : base()
        {
            IsPrivate = isPrivate;
        }

        public bool UserIsInRoom(string userId) {
            return Connections.Any(c => c.Connection.UserId == userId);
        }

        public int ActiveUserCount() {
            return Connections.Distinct(new ChatRoom_ApplicationUserConnection_UserComparer()).Count();
        }

        public bool OnlySpecifiedUsersInRoom(string[] userId) {
            return AllowedUserIds.All(i => userId.Any(id => id == i));
            //return Connections.TakeWhile((c, index) => userId.Any(u => u == c.UserId)).Count() == Connections.Count();
        }
    }

    public class ChatRoomViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColor { get; set; }
    }
}