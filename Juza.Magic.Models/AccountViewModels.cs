using System.ComponentModel.DataAnnotations;

namespace Juza.Magic.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        //public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(25, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "Player")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }



    //public class LoginViewModel : AbstractExtensions, IViewModel
    //{
    //    [Required]
    //    [Display(Name = "Player")]
    //    public string UserName { get; set; }

    //    [Required]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [Display(Name = "Remember me?")]
    //    public bool RememberMe { get; set; }
    //}

    //public class ExternalLoginConfirmationViewModel : AbstractExtensions, IViewModel
    //{
    //    [Required(ErrorMessage = "You must enter an username to be able to log in.")]
    //    [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
    //    [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
    //    [Display(Name = "New Player")]
    //    public string UserName { get; set; }

    //    [EmailAddress(ErrorMessage = "The email doesn't seem valid...")]
    //    [DataType(DataType.EmailAddress)]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [BirthDateRange(ErrorMessage = "The date doesn't seem valid...")]
    //    [DataType(DataType.Date, ErrorMessage = "You need to enter a date in format similar to 31/12/2000.")]
    //    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    //    [Display(Name = "Your Birthday")]
    //    public DateTime? BirthDate { get; set; }

    //    [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}

    //public class RegisterViewModel : AbstractExtensions, IViewModel
    //{
    //    [Required(ErrorMessage = "You must enter an username to be able to log in.")]
    //    [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
    //    [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
    //    [Display(Name = "New Player")]
    //    public string UserName { get; set; }

    //    [EmailAddress(ErrorMessage = "The email doesn't seem valid...")]
    //    [DataType(DataType.EmailAddress)]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }

    //    [BirthDateRange(ErrorMessage = "The date doesn't seem valid...")]
    //    [DataType(DataType.Date, ErrorMessage = "You need to enter a date in format similar to 31/12/2000.")]
    //    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    //    [Display(Name = "Your Birthday")]
    //    public DateTime? BirthDate { get; set; }

    //    [Required(ErrorMessage = "You must set a password to be able to log in.")]
    //    [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}
}
