using System.Collections.Generic;

namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatRoomConnection
    {
        public string ChatRoomId { get; set; }
        public int UserId { get; set; }
        public string ConnectionId { get; set; }

        public ChatRoom ChatRoom { get; set; }
        public User User { get; set; }
        public UserConnection Connection { get; set; }
    }

    public class ChatRoomConnection_ChatRoomComparer : IEqualityComparer<ChatRoomConnection>
    {
        public bool Equals(ChatRoomConnection x, ChatRoomConnection y)
        {
            return x.ChatRoomId == y.ChatRoomId;
        }

        public int GetHashCode(ChatRoomConnection obj)
        {
            return obj.ChatRoomId.GetHashCode();
        }
    }

    public class ChatRoomConnection_UserComparer : IEqualityComparer<ChatRoomConnection>
    {
        public bool Equals(ChatRoomConnection x, ChatRoomConnection y)
        {
            return x.UserId == y.UserId;
        }

        public int GetHashCode(ChatRoomConnection obj)
        {
            return obj.UserId.GetHashCode();
        }
    }
}