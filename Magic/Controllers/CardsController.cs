using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Controllers
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

        #region CREATE/EDIT
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.IsUpdate = false;
            return View("CreateOrEdit");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string errorText;
            var model =  context.Read<Card, int>(id, out errorText);
            TempData["Error"] = errorText;

            ViewBag.IsUpdate = true;
            return View("CreateOrEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(Card model, bool isUpdate = false) //[Bind(Include = "Id, Name")] 
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