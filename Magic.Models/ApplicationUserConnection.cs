using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace Magic.Models
{
    public class ApplicationUserConnection : AbstractExtensions
    {
        public string Id { get; set; }
        public virtual ApplicationUser User { get; set; }
        //public virtual IList<ChatRoom> ChatRooms { get; set; }
    }

    public class ApplicationUserGameConnection : ApplicationUserConnection
    {
        public virtual Game Game { get; set; }
    }

    public class ApplicationUserConnection_UserComparer : IEqualityComparer<ApplicationUserConnection>
    {
        public bool Equals(ApplicationUserConnection x, ApplicationUserConnection y)
        {
            return x.User == y.User;
        }

        public int GetHashCode(ApplicationUserConnection obj)
        {
            return obj.User.GetHashCode();
        }
    }
}