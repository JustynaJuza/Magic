using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Magic.Models.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Magic.Models;
using Magic.Models.DataContext;
using System.Data.Entity;
using Magic.Hubs;
using System.IO;

namespace Magic.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        public UserManager<ApplicationUser> UserManager { get; private set; }
        public AccountController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new MagicDbContext()));
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
                ApplicationUser user = new ApplicationUser()
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
                EditErrorMessageConfirmPassword();
                EditErrorMessageBirthDate();
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
                ModelState.AddModelError("", "");
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
                EditErrorMessageConfirmPassword();
                EditErrorMessageBirthDate();
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

            TempData["Error"] = "There was an error while linking your " + loginInfo.Login.LoginProvider + " account. Have you already associated it with a different account?";
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
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var foundUser = context.Users.Find(userId);

                //ChatHub.ToggleChatSubscription(foundUser);
                //foundUser.Connections.Clear();
                //foundUser.Status = UserStatus.Offline;
                //context.InsertOrUpdate(foundUser);

                AuthenticationManager.SignOut();
            }

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
            ViewBag.HasLocalPassword = HasPassword();

            var foundUser = UserManager.FindById(User.Identity.GetUserId());
            if (foundUser == null)
            {
                TempData["Error"] = "There was an error while accessing your profile. You are probably no longer logged in.";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Manage") });
            }

            // Check for Model error processing from partial views.
            if (TempData["PasswordViewData"] != null)
                ViewBag.PasswordViewData = TempData["PasswordViewData"];
            if (TempData["DetailsViewData"] != null)
                ViewBag.DetailsViewData = TempData["DetailsViewData"];
            if (TempData["ImageViewData"] != null)
                ViewBag.ImageViewData = TempData["ImageViewData"];

            // Determine with which loginProviders user can still associate.
            var availableLoginProviders = HttpContext.ApplicationInstance.Context.GetOwinContext().Authentication.GetExternalAuthenticationTypes();
            var loginProviders = availableLoginProviders.Where(provider => foundUser.Logins.All(l => l.LoginProvider != provider.AuthenticationType)).ToList();
            ViewBag.LoginProviders = loginProviders.Count > 0 ? loginProviders : null;

            return View(foundUser.GetViewModel());
        }

        public ActionResult ManageUserColor()
        {
            var foundUser = UserManager.FindById(User.Identity.GetUserId());
            foundUser.ColorCode = String.Empty.AssignRandomColorCode();

            TempData["Error"] = context.InsertOrUpdate(foundUser);
            return RedirectToAction("Manage");
        }

        public ActionResult ManageUserImage(HttpPostedFileBase file)
        {
            ViewBag.HasLocalPassword = HasPassword();

            if (file != null)
            {
                bool imageFile = System.Text.RegularExpressions.Regex.IsMatch(file.ContentType, "image");
                if (!imageFile)
                {
                    // Invalid file type - make model invalid.
                    ModelState.AddModelError("Image", "This must be an image file.");
                }

                if (ModelState.IsValid)
                {
                    var foundUser = UserManager.FindById(User.Identity.GetUserId());

                    var extension = file.FileName.Split('.').Last();
                    var imagePath = "~/Content/Images/Users/" + foundUser.UserName + "." + extension;
                    file.SaveAs(Server.MapPath(imagePath));

                    foundUser.Image = imagePath;

                    TempData["Error"] = context.InsertOrUpdate(foundUser);
                }
            }

            // Pass and process model errors.
            TempData["ImageViewData"] = ViewData;
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageUserDetails(UserViewModel model)
        {
            ViewBag.HasLocalPassword = HasPassword();

            if (ModelState.IsValid)
            {
                var foundUser = UserManager.FindById(User.Identity.GetUserId());
                foundUser.BirthDate = model.BirthDate;
                foundUser.Email = model.Email;

                TempData["Error"] = context.InsertOrUpdate(foundUser);
                if (TempData["Error"] == null)
                {
                    TempData["Message"] = "Your details have been updated.";
                }
            }
            else
            {
                EditErrorMessageBirthDate();
            }

            // Pass and process model errors.
            TempData["DetailsViewData"] = ViewData;
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManagePassword(ManagePasswordViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;

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
                    EditErrorMessageConfirmPassword();
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
                    EditErrorMessageConfirmPassword();
                }
            }
            // Pass and process model errors.
            TempData["PasswordViewData"] = ViewData;
            return RedirectToAction("Manage");
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
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

        private void EditErrorMessageConfirmPassword()
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

        private void EditErrorMessageBirthDate()
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
            context.InsertOrUpdate(user);
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