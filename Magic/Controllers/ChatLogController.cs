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

        [HttpGet]
        public ActionResult View(ChatLog model)
        {
            TempData["Error"] = context.Read(model);
            if (TempData["Error"].GetType() == typeof(string))
            {
                return RedirectToAction("Index");
            }
            TempData["Error"] = null;
            System.Diagnostics.Debug.WriteLine(model.ToString());
            return View("MessageLog", context.Set<ChatMessage>().Where(m => m.Log.DateCreated == model.DateCreated));
        }

		#region DELETE
        [HttpPost]
        public ActionResult Delete(Object model)
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