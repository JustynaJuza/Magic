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
    public class CardsController : Controller
    {
        private MagicDBContext context = new MagicDBContext();

        [HttpGet]
        public ViewResult Index()
        {
            return View(context.Set<Card>().ToList());
        }

        #region CREATE
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Card actionItem)
        {
            if (ModelState.IsValid)
            {
                Card item = new Card()
                {
                    Name = actionItem.Name
                };

                TempData["Error"] = context.Create(item);
                return RedirectToAction("Index");
            }
            // Process model errors.
            return View(actionItem);
        }
        #endregion CREATE

        #region EDIT/UPDATE
        [HttpGet]
        public ActionResult Edit(Card actionItem)
        {
            TempData["Error"] = context.Read(actionItem);
            if (TempData["Error"].GetType() == typeof(string))
            {
                return RedirectToAction("Index");
            }
            return View(actionItem);
        }

        [HttpPost]
        public ActionResult PostEdit([Bind(Include = "Id, Name")] Card actionItem)
        {
            if (ModelState.IsValid)
            {
                TempData["Error"] = context.Update(actionItem);
                return RedirectToAction("Index");
            }
            // Process model errors.
            return View("Edit", actionItem);
        }
        #endregion EDIT/UPDATE

        #region DELETE
        public ActionResult Delete(Card actionItem)
        {
            TempData["Error"] = context.Delete(actionItem);
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