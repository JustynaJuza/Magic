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
        public string TabColorCode { get; set; }
        public virtual ChatLog Log { get; set; }
        public virtual IList<ChatRoom_ApplicationUserConnection> Connections { get; set; }

        public ChatRoom() {
            Id = Guid.NewGuid().ToString();
            TabColorCode = String.Empty.AssignRandomColorCode();
            Log = new ChatLog();
            Connections = new List<ChatRoom_ApplicationUserConnection>();
        }

        public bool UserIsInRoom(string userId) {
            return Connections.Any(c => c.Connection.UserId == userId);
        }
    }

    public class ChatRoomViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColor { get; set; }
    }
}