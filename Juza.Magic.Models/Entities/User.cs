using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Enums;
using Juza.Magic.Models.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Juza.Magic.Models.Entities
{
    public enum SystemRole
    {
        Developer,
        Admin
    }

    public class User : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {
        public DateTime DateCreated { get; set; }

        [DisplayFormat(NullDisplayText = "Never connected")]
        public DateTime? LastLoginDate { get; set; }

        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public UserStatus Status { get; set; }

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

            return classMembers.Aggregate(toString,
                (current, member) => current + ("\n" + member.Name + " : " + member.GetValue(this) + "; "));
        }

        #endregion HELPERS

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            //if (CorporateClientId.HasValue)
            //    userIdentity.AddClaim(new Claim(ClaimType.CorporateClientId, CorporateClientId.ToString()));

            return userIdentity;
        }
    }

    public static class UserQueryExtensions
    {
        public static IQueryable<User> GetFriends(this IQueryable<User> query)
        {
            return query.Select(x => x.Relations).OfType<UserRelationFriend>().Select(x => x.RelatedUser);
        }

        public static IQueryable<User> GetIgnored(this IQueryable<User> query)
        {
            return query.Select(x => x.Relations).OfType<UserRelationIgnored>().Select(x => x.RelatedUser);
        }
    }

    // IdentityDbContext included in DataContext namespace => see MagicDBContext.
}