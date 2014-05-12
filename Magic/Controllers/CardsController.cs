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

        #region CREATE
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Card model)
        {
            if (ModelState.IsValid)
            {
                //Card item = new Card()
                //{
                //    Name = model.Name
                //};

                TempData["Error"] = context.Create(model);
                return RedirectToAction("Index");
            }
            // Process model errors.
            return View(model);
        }
        #endregion CREATE

        #region EDIT/UPDATE
        [HttpGet]
        public ActionResult Edit(Card model)
        {
            TempData["Error"] = context.Read(model);
            if (TempData["Error"].GetType() == typeof(string))
            {
                return RedirectToAction("Index");
            }
            TempData["Error"] = null;
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult PostEdit([Bind(Include = "Id, Name")] Card model)
        {
            if (ModelState.IsValid)
            {
                TempData["Error"] = context.Update(model);
                return RedirectToAction("Index");
            }
            // Process model errors.
            return View("Edit", model);
        }
        #endregion EDIT/UPDATE

        #region DELETE
        public ActionResult Delete(Card model)
        {
            TempData["Error"] = context.Delete(model);
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