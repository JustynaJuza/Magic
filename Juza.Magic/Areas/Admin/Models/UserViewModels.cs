using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Juza.Magic.Models.Entities;

namespace Juza.Magic.Areas.Admin.Models
{
    public class SetUserViewModel
    {
        public int Id { get; set; }

        [DisplayFormat(NullDisplayText = "Has not logged in yet")]
        public DateTime? LastLoginDate { get; private set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Username (Email)")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The text entered is too long, consider using your initials instead.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        [StringLength(100, ErrorMessage = "The text entered is too long.")]
        public string LastName { get; set; }

        [Display(Name = "Corporate client")]
        public int? CorporateClientId { get; set; }

        [Display(Name = "User roles")]
        public IEnumerable<int> Roles { get; set; }

        public SetUserViewModel()
        {
            Roles = Enumerable.Empty<int>();
        }

        public SetUserViewModel(User user)
        {
            Id = user.Id;
            LastLoginDate = user.LastLoginDate;
            UserName = user.UserName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Roles = user.Roles.Select(r => r.RoleId);
        }
    }

    public class SetUserRolesViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Roles")]
        public IEnumerable<int> Roles { get; set; }
    }

    public class SetUserNameViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }
    }
}