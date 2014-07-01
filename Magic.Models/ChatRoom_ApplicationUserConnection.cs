using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class ChatRoom_ApplicationUserConnection
    {
        public string ChatRoomId { get; set; }
        public string ConnectionId { get; set; }
        public string UserId { get; set; }

        public ChatRoom ChatRoom { get; set; }
        public ApplicationUserConnection Connection { get; set; }
    }

    public class ChatRoom_ApplicationUserConnection_ChatRoomComparer : IEqualityComparer<ChatRoom_ApplicationUserConnection>
    {
        public bool Equals(ChatRoom_ApplicationUserConnection x, ChatRoom_ApplicationUserConnection y)
        {
            return x.ChatRoomId == y.ChatRoomId;
        }

        public int GetHashCode(ChatRoom_ApplicationUserConnection obj)
        {
            return obj.ChatRoomId.GetHashCode();
        }
    }

    public class ChatRoom_ApplicationUserConnection_UserComparer : IEqualityComparer<ChatRoom_ApplicationUserConnection>
    {
        public bool Equals(ChatRoom_ApplicationUserConnection x, ChatRoom_ApplicationUserConnection y)
        {
            return x.UserId == y.UserId;
        }

        public int GetHashCode(ChatRoom_ApplicationUserConnection obj)
        {
            return obj.UserId.GetHashCode();
        }
    }
}