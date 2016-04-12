using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Juza.Magic.Areas.Admin.Models;
using Juza.Magic.Models;
using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Juza.Magic.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly IAuthenticationManager _authenticationManager;

        public UsersController(IDbContext context,
            ApplicationUserManager userManager,
            IAuthenticationManager authenticationManager)
        {
            _context = context;
            _userManager = userManager;
            _authenticationManager = authenticationManager;
        }

        public ActionResult Index()
        {
            var usersWithRoles = _context.Set<User>()
                .Include(u => u.Roles.Select(r => r.Role));
            return View(usersWithRoles.ToList());
        }

        #region CREATE/EDIT
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.IsUpdate = false;
            return View("CreateOrEdit");
        }

        [HttpGet]
        public ActionResult Edit(int id = 0)
        {
            if (id != 0)
            {
                var model = _context.Read<User>(id);
                if (model != null)
                {
                    ViewBag.IsUpdate = true;
                    return View("CreateOrEdit", new UserViewModel(model));
                }

                TempData["Error"] = "This user no longer exists!";
                return RedirectToAction("Index");
            }

            TempData["Message"] = "There was no item Id provided for editing, assuming creation of new item.";
            return RedirectToAction("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public ActionResult Insert([Bind(Exclude = "Id")] SetUserViewModel viewModel)
        {
            if (!IsUserNameAvailable(viewModel.UserName))
            {
                ModelState.AddModelError("UserName", "The Username (Email) " + viewModel.UserName + " is already in use.");

                ViewBag.IsUpdate = false;
                return View("CreateOrEdit", viewModel);
            }

            var user = new User
            {
                UserName = viewModel.UserName,
                Email = viewModel.UserName,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName
            };

            SetUserRoles(user, viewModel.Roles);
            _context.InsertAndSave(user);

            //var entityId = _context.Entry(user).Entity.Id;
            //return RedirectToAction("Edit", new { id = entityId });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult Update(SetUserViewModel viewModel)
        {
            ViewBag.IsUpdate = true;

            if (!ModelState.IsValid)
                return View("CreateOrEdit", viewModel);

            var user = _context.Read<User>(viewModel.Id);
            if (user == null)
            {
                TempData["Error"] = "This user no longer exists!";
                return RedirectToAction("Index");
            }

            var hasRequestedUserNameChange = user.UserName != viewModel.UserName;
            if (hasRequestedUserNameChange)
            {
                if (IsUserNameAvailable(viewModel.UserName))
                {
                    user.UserName = viewModel.UserName;
                    user.Email = viewModel.UserName;
                }
                else
                {
                    ModelState.AddModelError("UserName", "The username " + viewModel.UserName + " is already in use.");
                    viewModel.UserName = user.UserName;
                    return View("CreateOrEdit", viewModel);
                }
            }

            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;

            SetUserRoles(user, viewModel.Roles);

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public bool IsUserNameAvailable(string userName)
        {
            return _context.Set<User>().FirstOrDefault(u => u.UserName == userName) == null;
        }

        #endregion CREATE/EDIT

        #region DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                _context.FindAndDeleteAndSave<User, int>(id);
                return new JsonResult { Data = new SuccessResult { Success = true } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new SuccessResult { Success = false, Exception = ex } };
            }
        }
        #endregion DELETE

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUserRoles(SetUserRolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Read<User>(model.Id);
                if (user == null)
                {
                    TempData["Error"] = "This user no longer exists!";
                    return RedirectToAction("Index", "Users");
                }

                SetUserRoles(user, model.Roles);

                _context.SaveChanges();
                ViewBag.IsSuccess = true;
            }

            return PartialView("~/Views/Users/_SetUserRolesPartial.cshtml", model);
        }

        private void SetUserRoles(User user, IEnumerable<int> selectedRoles)
        {
            var rolesToRemove = user.Roles.Where(r => !selectedRoles.Contains(r.RoleId)).ToList();
            var roleIdsToAdd = selectedRoles.Except(user.Roles.Select(r => r.RoleId));
            foreach (var role in rolesToRemove)
            {
                user.Roles.Remove(role);
            }
            foreach (var role in roleIdsToAdd)
            {
                user.Roles.Add(new UserRole { RoleId = role });
            }
        }

        public JsonResult ResetUserPassword(int id)
        {
            var user = _userManager.FindById(id);
            if (user == null)
            {
                return new JsonResult
                {
                    Data = new SuccessResult
                    {
                        Description = "This user no longer exists!",
                        Success = false
                    }
                };
            }

            var code = _userManager.GeneratePasswordResetToken(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            
            var isSent = SendEmail(user.Email, callbackUrl);

            return new JsonResult
            {
                Data = new SuccessResult
                {
                    Description = "This user no longer exists!",
                    Success = isSent
                }
            };
        }

        private bool SendEmail(string email, string link)
        {
            var client = new SmtpClient();
            client.Send(new MailMessage(
                new MailAddress("noreply-notifications@eSess.penna.com", "Penna eSess"),
                new MailAddress(email))
            {
                Subject = "Welcome to Penna eSess",
                Body = "<p style=\"font-family:Arial\">Please use this link to set your password: " + link + "</p>",
                IsBodyHtml = true
            });

            return true;
        }
    }
}