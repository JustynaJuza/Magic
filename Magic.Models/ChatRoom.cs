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
        public virtual IList<ApplicationUserConnection> UserConnections { get; set; }

        public ChatRoom() {
            Id = Guid.NewGuid().ToString();
            TabColorCode.AssignRandomColorCode();
            Log = new ChatLog();
            UserConnections = new List<ApplicationUserConnection>();
        }
    }

    public class ChatRoomViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TabColor { get; set; }
    }
}