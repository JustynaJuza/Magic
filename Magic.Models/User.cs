using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Magic.Models.Helpers;

namespace Magic.Models
{
    public enum Role
    {
        Developer,
        Admin
    }

    public class User : IdentityUser
    {
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
        public virtual IList<CardDeck> DecksCreated { get; set; }
        public virtual IList<UserConnection> Connections { get; set; }
        public virtual IList<GamePlayerStatus> Games { get; set; }
        public virtual IList<ChatMessageNotification> ChatMessages { get; set; }
        public virtual IList<UserRelation> Relations { get; set; }
        //public virtual IList<UserRelationFriend> Friends { get; set; }
        //public virtual IList<UserRelationIgnored> Ignored { get; set; }

        // Constructor.
        public User()
        {
            ColorCode = String.Empty.AssignRandomColorCode();
            Connections = new List<UserConnection>();
            DeckCollection = new List<CardDeck>();
            IsFemale = false;
        }

        #region HELPERS
        public IList<UserRelationFriend> GetFriends()
        {
            return Relations.OfType<UserRelationFriend>().ToList();
        }

        public IList<ChatUserViewModel> GetFriendsList()
        {
            return GetFriends().Select(f => f.RelatedUser).Select(user => new ChatUserViewModel(user)).ToList();
        }

        public IList<UserRelationIgnored> GetIgnored()
        {
            return Relations.OfType<UserRelationIgnored>().ToList();
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
            string toString = GetType().FullName + ": ";
            var classMembers = GetType().GetProperties();

            return classMembers.Aggregate(toString, (current, member) => current + ("\n" + member.Name + " : " + member.GetValue(this) + "; "));
        }
        #endregion HELPERS
    }
    // IdentityDbContext included in DataContext namespace => see MagicDBContext.
}