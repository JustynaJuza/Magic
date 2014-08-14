using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Magic.Models.Helpers;

namespace Magic.Models
{
    public class ChatRoomUserConnection : AbstractExtensions
    {
        public string ChatRoomId { get; set; }
        public string UserId { get; set; }
        public string ConnectionId { get; set; }

        public ChatRoom ChatRoom { get; set; }
        public User User { get; set; }
        public UserConnection Connection { get; set; }
    }

    public class ChatRoom_ApplicationUserConnection_ChatRoomComparer : IEqualityComparer<ChatRoomUserConnection>
    {
        public bool Equals(ChatRoomUserConnection x, ChatRoomUserConnection y)
        {
            return x.ChatRoomId == y.ChatRoomId;
        }

        public int GetHashCode(ChatRoomUserConnection obj)
        {
            return obj.ChatRoomId.GetHashCode();
        }
    }

    public class ChatRoom_ApplicationUserConnection_UserComparer : IEqualityComparer<ChatRoomUserConnection>
    {
        public bool Equals(ChatRoomUserConnection x, ChatRoomUserConnection y)
        {
            return x.UserId == y.UserId;
        }

        public int GetHashCode(ChatRoomUserConnection obj)
        {
            return obj.UserId.GetHashCode();
        }
    }
}