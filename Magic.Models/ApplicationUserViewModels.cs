using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Magic.Models
{
    public class UserViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; private set; }

        [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
        [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; private set; }

        public string Title { get; set; }

        [EmailAddress(ErrorMessage = "The email doesn't seem valid...")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [BirthDateRange(ErrorMessage = "The date doesn't seem valid...")]
        [DataType(DataType.Date, ErrorMessage = "You need to enter a date in format similar to 31/12/2000.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Your Birthday")]
        public DateTime? BirthDate { get; set; }

        [DataType(DataType.ImageUrl)]
        [Display(Name = "Your Plainswalker")]
        public string Image { get; set; }

        public string ColorCode { get; private set; }

        // Constructor.
        public UserViewModel(){ }
        // Constructor.
        public UserViewModel(ApplicationUser user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Title = user.Title;
            Email = user.Email;
            BirthDate = user.BirthDate;
            Image = user.Image;
            ColorCode = user.ColorCode;
        }
    }

    public class ChatUserViewModel
    {
        public string UserName { get; set; }
        public string ColorCode { get; set; }
        public UserStatus Status { get; set; }

        public ChatUserViewModel(ApplicationUser user)
        {
            UserName = user.UserName;
            ColorCode = user.ColorCode;
            Status = user.Status;
        }
    }

    public class ManagePasswordViewModel : AbstractExtensions, IViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "You must set a password to be able to log in.")]
        [StringLength(100, ErrorMessage = "The new password must be at least 6 characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel : AbstractExtensions, IViewModel
    {
        [Required]
        [Display(Name = "Player")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class ExternalLoginConfirmationViewModel : AbstractExtensions, IViewModel
    {
        [Required(ErrorMessage = "You must enter an username to be able to log in.")]
        [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
        [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "The email doesn't seem valid...")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [BirthDateRange(ErrorMessage = "The date doesn't seem valid...")]
        [DataType(DataType.Date, ErrorMessage = "You need to enter a date in format similar to 31/12/2000.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Your Birthday")]
        public DateTime? BirthDate { get; set; }

        [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterViewModel : AbstractExtensions, IViewModel
    {
        [Required(ErrorMessage = "You must enter an username to be able to log in.")]
        [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
        [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "The email doesn't seem valid...")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [BirthDateRange(ErrorMessage = "The date doesn't seem valid...")]
        [DataType(DataType.Date, ErrorMessage = "You need to enter a date in format similar to 31/12/2000.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Your Birthday")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "You must set a password to be able to log in.")]
        [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    // Specifies an attribute that checks the BirthDate range.
    public class BirthDateRange : RangeAttribute
    {
        private static string startDate = DateTime.Now.AddYears(-100).ToString();
        private static string endDate = DateTime.Today.ToString();

        public BirthDateRange()
            : base(typeof(DateTime), startDate, endDate) { }
    }
}
