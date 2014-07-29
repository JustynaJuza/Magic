using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Magic.Models.Helpers;

namespace Magic.Models
{
    public class ApplicationUser : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; }
        [DisplayFormat(NullDisplayText = "Never connected")]
        public DateTime? LastLoginDate { get; set; }
        public string Title { get; set; }
        public UserStatus Status { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        public bool IsFemale { get; set; }
        public string Country { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        public string ColorCode { get; set; }
        public virtual IList<CardDeck> DeckCollection { get; set; }
        public virtual IList<ApplicationUserConnection> Connections { get; set; }
        public virtual IList<Player_GameStatus> Games { get; set; }
        public virtual IList<Recipient_ChatMessageStatus> ChatMessages { get; set; }
        public virtual IList<ApplicationUserRelation> Relations { get; set; }
        //public virtual IList<ApplicationUserRelation_Friend> Friends { get; set; }
        //public virtual IList<ApplicationUserRelation_Ignored> Ignored { get; set; }

        // Constructor.
        public ApplicationUser()
        {
            ColorCode = String.Empty.AssignRandomColorCode();
            Connections = new List<ApplicationUserConnection>();
            DeckCollection = new List<CardDeck>();
            IsFemale = false;
        }

        #region HELPERS
        public IList<ApplicationUserRelation_Friend> GetFriends()
        {
            return Relations.OfType<ApplicationUserRelation_Friend>().ToList();
        }

        public IList<ApplicationUserRelation_Ignored> GetIgnored()
        {
            return Relations.OfType<ApplicationUserRelation_Ignored>().ToList();
        }

        public UserViewModel GetViewModel()
        {
            return new UserViewModel(this);
        }

        public ProfileViewModel GetProfileViewModel()
        {
            return new ProfileViewModel(this);
        }
        
        public override string ToString()
        {
            string toString = this.GetType().FullName + ": ";
            var classMembers = this.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(this) + "; ";

            return toString;
        }
        #endregion HELPERS
    }
    // IdentityDbContext included in DataContext namespace => see MagicDBContext.
}