using Juza.Magic.Models.Entities;

namespace Juza.Magic.Models.Chat
{
    public class ChatRoomUser
    {
        public string ChatRoomId { get; set; }
        public string UserId { get; set; }

        public ChatRoom ChatRoom { get; set; }
        public User User { get; set; }
    }
}