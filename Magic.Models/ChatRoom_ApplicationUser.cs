using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatRoom_ApplicationUser
    {
        public string ChatRoomId { get; set; }
        public string UserId { get; set; }

        public ChatRoom ChatRoom { get; set; }
        public ApplicationUser User { get; set; }
    }
}