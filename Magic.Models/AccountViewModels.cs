using System;
using System.ComponentModel.DataAnnotations;

namespace Magic.Models
{
    public class ExternalLoginConfirmationViewModel : AbstractToString
    {
        [Required]
        [StringLength(40, ErrorMessage = "Player name must be between 3-40 characters long.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9-?.?\s]*$", ErrorMessage = "A Player can be named only with letters, numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; set; }

        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //public string StartDate = DateTime.Now.AddYears(-100).ToString();
        //public string EndDate = DateTime.Today.ToString();
        [DataType(DataType.Date)]
        //[Range(typeof(DateTime), StartDate, EndDate, ErrorMessage = "The date value doesn't seem valid...")]
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
        [StringLength(40, ErrorMessage = "Player name must be between 3-40 characters long.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9-?.?\s]*$", ErrorMessage = "A Player can be named only with letters, numbers and a few selected special characters.")]
        [Display(Name = "Player")]
        public string UserName { get; set; }

        public string Title { get; set; }

        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Date)]
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

        [Required]
        [StringLength(100, ErrorMessage = "The new password must be at least 6 characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
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
        [Required]
        [StringLength(30, ErrorMessage = "Player name must be between 3-30 characters long.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9-?.?\s]*$", ErrorMessage = "A Player can be named only with letters, numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; set; }

        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Your Birthday")]
        public DateTime? BirthDate { get; set; }

        [Required]
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
