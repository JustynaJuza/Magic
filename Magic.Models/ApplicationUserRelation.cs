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
    public abstract class ApplicationUserRelation : AbstractExtensions
    {
        public string UserId { get; set; }
        public string RelatedUserId { get; set; }
        public ApplicationUser User { get; set; }
        public ApplicationUser RelatedUser { get; set; }
        //public UserRelationship Relationship  { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateSet { get; set; }
    }

    public class ApplicationUserRelation_Friend : ApplicationUserRelation
    { 
    
    }

    public class ApplicationUserRelation_Ignored : ApplicationUserRelation
    {

    }
}