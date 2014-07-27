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
    public class ApplicationUser_RelatedApplicationUser : AbstractExtensions
    {
        public string UserId { get; set; }
        public string RelatedUserId { get; set; }
        public ApplicationUser User { get; set; }
        public ApplicationUser RelatedUser { get; set; }
        //public UserRelationship Relationship  { get; set; }
        public DateTime DateSet { get; set; }
    }

    public class ApplicationUser_RelatedApplicationUser_Friend : ApplicationUser_RelatedApplicationUser
    { 
    
    }

    public class ApplicationUser_RelatedApplicationUser_Ignored : ApplicationUser_RelatedApplicationUser
    {

    }
}