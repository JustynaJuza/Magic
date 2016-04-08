using System.Collections.Generic;
using Juza.Magic.Models.Extensions;

namespace Juza.Magic.Models.Entities
{
    public class UserConnection : AbstractExtensions
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        //public virtual IList<ChatRoom> ChatRooms { get; set; }
        public string GameId { get; set; }
        public virtual Game Game { get; set; }
    }
    
    public class UserConnection_UserComparer : IEqualityComparer<UserConnection>
    {
        public bool Equals(UserConnection x, UserConnection y)
        {
            return x.User == y.User;
        }

        public int GetHashCode(UserConnection obj)
        {
            return obj.User.GetHashCode();
        }
    }
}