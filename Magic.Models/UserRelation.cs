using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Magic.Models.Helpers;

namespace Magic.Models
{
    public enum UserRelationship
    {
        Friend,
        Ignored
    }

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