using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class ApplicationUserRelation : AbstractExtensions
    {
        public string UserId { get; set; }
        public string RelatedUserId { get; set; }
        public ApplicationUser User { get; set; }
        public ApplicationUser RelatedUser { get; set; }
        //public UserRelationship Relationship  { get; set; }
        public DateTime DateSet { get; set; }
    }

    public class ApplicationUserRelation_Friend : ApplicationUserRelation
    { 
    
    }

    public class ApplicationUserRelation_Ignored : ApplicationUserRelation
    {

    }
}