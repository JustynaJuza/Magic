using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatRoom
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual IList<ApplicationUserConnection> UserConnections { get; set; }
        public virtual ChatLog Log { get; set; }

        public ChatRoom()
        {
            Log = new ChatLog();
            UserConnections = new List<ApplicationUserConnection>();
        }
    }
}