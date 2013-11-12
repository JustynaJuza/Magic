using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Controllers
{
    public class ChatLogController : Controller
    {
        private MagicDBContext context = new MagicDBContext();

		[HttpGet]
        public ActionResult Index()
        {
            return View(context.ChatLogs.ToList());
        }

		#region CREATE
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ChatLog actionItem)
        {
            if (ModelState.IsValid)
            {
                TempData["Error"] = context.Create(actionItem);
                return RedirectToAction("Index");
            }

            return View(actionItem);
        }
		#endregion CREATE

        #region EDIT/UPDATE
        [HttpGet]
        public ActionResult Edit(ChatLog actionItem)
        {
            TempData["Error"] = context.Read(actionItem);
            if (TempData["Error"].GetType() == typeof(string))
            {
                return RedirectToAction("Index");
            }

            return View(actionItem);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ChatLog actionItem)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ChatLog actionItem)
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