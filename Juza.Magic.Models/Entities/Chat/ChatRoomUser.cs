namespace Juza.Magic.Models.Entities.Chat
{
    public class ChatRoomUser
    {
        public string ChatRoomId { get; set; }
        public int UserId { get; set; }

        public ChatRoom ChatRoom { get; set; }
        public User User { get; set; }
    }
}