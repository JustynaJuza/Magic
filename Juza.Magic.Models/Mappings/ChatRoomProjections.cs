using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Projections;
using System.Linq;

namespace Juza.Magic.Models.Mappings
{
    public class ChatRoomToChatRoomViewModelProjection : SingleMapping<ChatRoom, ChatRoomViewModel>
    {
        public ChatRoomToChatRoomViewModelProjection() :
            base(chatRoom => new ChatRoomViewModel
            {
                Id = chatRoom.Id,
                Name = chatRoom.Name,
                IsGameRoom = chatRoom.IsGameRoom,
                IsPrivate = chatRoom.IsPrivate,
                TabColorCode = chatRoom.TabColorCode,
                Log = new ChatLogViewModel
                {
                    Id = chatRoom.Log.Id,
                    DateCreated = chatRoom.Log.DateCreated,
                    Messages = chatRoom.Log.Messages.Select(message => new ChatMessageViewModel
                    {
                        TimeSent = message.TimeSent,
                        Message = message.Message,
                        SenderName = message.Sender.UserName
                    })
                },
                Users = chatRoom.Users.Select(chatUser => new ChatUserViewModel
                {
                    Id = chatUser.UserId,
                    ColorCode = chatUser.User.ColorCode,
                    Status = chatUser.User.Status,
                    UserName = chatUser.User.UserName
                })
            })
        { }
    }
}
