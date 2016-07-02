using System;

namespace Juza.Magic.Models.Entities
{
    public abstract class UserRelation
    {
        public int UserId { get; set; }
        public int RelatedUserId { get; set; }
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