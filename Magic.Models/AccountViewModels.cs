using System;
using System.ComponentModel.DataAnnotations;

namespace Magic.Models
{
    // Specifies an attribute that checks the BirthDate range.
    public class BirthDateRange : RangeAttribute
    {
        private static string startDate = DateTime.Now.AddYears(-100).ToString();
        private static string endDate = DateTime.Today.ToString();

        public BirthDateRange()
            : base(typeof(DateTime), startDate, endDate) { }
    }

    public class ExternalLoginConfirmationViewModel : AbstractToString
    {
        [Required(ErrorMessage="You must enter an username to be able to log in.")]
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

    public class ManageUserDetailsViewModel : AbstractToString
    {
        [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
        [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; set; }

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
        [Display(Name = "Your Plainswalker image")]
        public string UserImage { get; set; }
    }

    public class ManagePasswordViewModel : AbstractToString
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

    public class LoginViewModel : AbstractToString
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

    public class RegisterViewModel : AbstractToString
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
}
