using System;
using Juza.Magic.Models.Extensions;

namespace Juza.Magic.Models.Entities
{
    public abstract class UserRelation : AbstractExtensions
    {
        public string UserId { get; set; }
        public string RelatedUserId { get; set; }
        public User User { get; set; }
        public User RelatedUser { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class UserRelationFriend : UserRelation
    { 
    
    }

    public class UserRelationIgnored : UserRelation
    {

    }
}