using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Projections;

namespace Juza.Magic.Models.Mappings
{
    public class UserToChatUserViewModelProjection : SingleMapping<User, ChatUserViewModel>
    {
        public UserToChatUserViewModelProjection() :
            base(user => new ChatUserViewModel
            {
                Id = user.Id,
                ColorCode = user.ColorCode,
                Status = user.Status,
                UserName = user.UserName
            })
        { }
    }
}
