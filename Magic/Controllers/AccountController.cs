using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Magic.Models;
using Magic.Models.DataContext;
using System.Data.Entity;

namespace Magic.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private MagicDBContext context = new MagicDBContext();

        public UserManager<ApplicationUser> UserManager { get; private set; }
        public AccountController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new MagicDBContext()));
        }

        #region REGISTER
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    BirthDate = model.BirthDate
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                editErrorMessageConfirmPassword();
                editErrorMessageBirthDate();
            }
            // Process model errors.
            return View(model);
        }
        #endregion REGISTER

        #region LOG IN / LOGIN LINK / LOG OFF
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                //Invalid username/password combination - make model invalid.
                ModelState.AddModelError("","");
            }
            // Process model errors.
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider.
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account.
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                string email = "";
                if (loginInfo.Login.LoginProvider == "Google")
                {
                    email = loginInfo.DefaultUserName.ToLower() + "@gmail.com";
                }
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName, Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    BirthDate = model.BirthDate
                };

                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
            }
            else
            {
                editErrorMessageConfirmPassword();
                editErrorMessageBirthDate();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user.
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                TempData["Error"] = "The service was unable to get your social account data. Are you logged in on your social account?";
                return RedirectToAction("Manage");
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                TempData["Message"] = "You can now log in with " + loginInfo.Login.LoginProvider;
                return RedirectToAction("Manage");
            }

            TempData["Error"] = "There was an error while linking your " + loginInfo.Login.LoginProvider + " account, maybe try again later...";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                TempData["Message"] = "Your " + loginProvider + " login was removed.";
            }
            else
            {
                TempData["Error"] = "There was an error while disassociating your " + loginProvider + " account, maybe try again later...";
            }
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        #endregion LOG IN / LOGIN LINK / LOG OFF

        #region MANAGE
        [HttpGet]
        public ActionResult Manage()
        {
            var foundUser = UserManager.FindById(User.Identity.GetUserId());
            if (foundUser == null)
            {
                TempData["Error"] = "There was an error while accessing your profile. You are probably no longer logged in.";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Manage") });
            }

            ManageUserDetailsViewModel userDetails = new ManageUserDetailsViewModel
            {
                UserName = foundUser.UserName,
                Email = foundUser.Email,
                BirthDate = foundUser.BirthDate,
                UserImage = foundUser.UserImage
            };

            if (TempData["PasswordViewData"] != null)
                ViewBag.PasswordViewData = TempData["PasswordViewData"];
            if (TempData["DetailsViewData"] != null)
                ViewBag.DetailsViewData = TempData["DetailsViewData"];

            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View(userDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManagePassword(ManagePasswordViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Your password has been changed.";
                        return RedirectToAction("Manage");
                    }
                }
                else
                {
                    editErrorMessageConfirmPassword();
                    if (ModelState["OldPassword"].Errors.Count != 0)
                    {
                        ModelState["Password"].Errors.Clear();
                        ModelState["ConfirmPassword"].Errors.Clear();
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Your new password has been set.";
                        return RedirectToAction("Manage");
                    }
                }
                else
                {
                    editErrorMessageConfirmPassword();
                }
            }
            // Pass and process model errors.
            TempData["PasswordViewData"] = ViewData;
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageUserDetails(ManageUserDetailsViewModel model)
        {
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (ModelState.IsValid)
            {
                var currentUserID = User.Identity.GetUserId();
                var foundUser = context.Users.FirstOrDefault(u => u.Id == currentUserID);

                try
                {
                    foundUser.Title = model.Title;
                    foundUser.Email = model.Email;
                    foundUser.BirthDate = model.BirthDate;
                    foundUser.UserImage = model.UserImage;

                    context.Entry(foundUser).State = EntityState.Modified;
                    context.SaveChanges();

                    TempData["Message"] = "Your details have been updated.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Something went wrong here... That's quite unusual, maybe try again.";
                    ViewBag.ErrorLog = ex.ToFullString();
                    ViewBag.ErrorLog2 = ex.ToString();
                }
            }
            else
            {
                editErrorMessageBirthDate();
            }
            // Pass and process model errors.
            TempData["DetailsViewData"] = ViewData;
            return RedirectToAction("Manage");
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult) PartialView("_RemoveAccountPartial", linkedAccounts);
        }
        #endregion MANAGE

        #region DISPOSE
        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }
        #endregion DISPOSE

        #region HELPERS
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void editErrorMessageConfirmPassword()
        {
            if (ModelState["ConfirmPassword"].Errors.Count > 0)
            {
                var modelCompareError = ModelState["ConfirmPassword"].Errors.FirstOrDefault(e => e.ErrorMessage.Contains("'Confirm password'"));

                if (modelCompareError != null)
                {
                    ModelState["ConfirmPassword"].Errors.Remove(modelCompareError);
                    ModelState["ConfirmPassword"].Errors.Add(new ModelError("The password and confirmation password do not match."));
                }

                if (ModelState["Password"].Errors.Count != 0)
                    ModelState["ConfirmPassword"].Errors.Clear();
            }
        }

        private void editErrorMessageBirthDate()
        {
            if (ModelState["BirthDate"].Errors.Count > 0)
            {
                var modelInvalidError = ModelState["BirthDate"].Errors.FirstOrDefault(e => e.ErrorMessage.Contains("is not valid"));
                if (modelInvalidError != null)
                {
                    ModelState["BirthDate"].Errors.Remove(modelInvalidError);
                    ModelState["BirthDate"].Errors.Add(new ModelError("You should enter a correct date in format similar to 31/12/2000."));
                }
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);

            user.LastLoginDate = DateTime.Now;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion HELPERS
    }
}