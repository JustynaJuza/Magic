using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Magic.Hubs;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Areas.Admin.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        [HttpGet]
        public ViewResult Index()
        {
            return View(context.Cards.ToList());
        }

        [HttpPost]
        public ViewResult Index(IList<string> ids)
        {
            return View(context.Cards.ToList());
        }
        
        //[AsyncTimeout(200)]
        //public async void FetchCardsAsync()
        //{
        //    AdminHub.MakeCardsRequest();
        //}

        #region CREATE/EDIT
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.IsUpdate = false;
            return View("CreateOrEdit");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id != null)
            {
                string errorText;
                // TODO: Fix this or discard?
                //var model = context.Read<Card, string>((string) id, out errorText);
                //TempData["Error"] = errorText;
                var model = context.Cards.Find(id);
                if (model != null)
                {
                    ViewBag.IsUpdate = true;
                    return View("CreateOrEdit", model);
                }

                TempData["Error"] = MagicDbContext.ShowErrorMessage(new ArgumentNullException());
                return RedirectToAction("Index");
            }

            TempData["Message"] = "There was no item ID provided for editing, assuming creation of new item.";

            ViewBag.IsUpdate = false;
            return View("CreateOrEdit");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public ActionResult Insert(Card model, bool isUpdate = false)
        {
            model.Id = model.Name.ToLower().Replace(" ", "_").Replace("[^a-z0-9]*", ""); //Guid.NewGuid().ToString();
            return InsertOrUpdate(model, isUpdate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult Update(Card model, bool isUpdate = false) //[Bind(Include = "Id, Name")] 
        {
            return InsertOrUpdate(model, isUpdate);
        }

        private ActionResult InsertOrUpdate(Card model, bool isUpdate = false)
        {
            if (ModelState.IsValid)
            {
                string errorText;
                TempData["Error"] = context.InsertOrUpdate(model, out errorText) ? null : errorText;
                return RedirectToAction("Index");
            }

            // Process model errors.
            ViewBag.IsUpdate = isUpdate;
            return View("CreateOrEdit", model);
        }
        #endregion CREATE/EDIT

        #region DELETE
        public ActionResult Delete(Card model)
        {
            string errorText;
            TempData["Error"] = context.Delete(model, out errorText) ? null : errorText;
            return RedirectToAction("Index");
        }
        #endregion DELETE

        #region DISPOSE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion DISPOSE
    }
}