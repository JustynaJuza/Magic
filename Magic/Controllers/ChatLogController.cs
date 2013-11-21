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
            return View(context.ChatLogs.Include(l => l.MessageLog).ToList());
        }

        public ActionResult MessageLog(ChatLog model)
        {
            TempData["Error"] = context.Read(model);
            if (TempData["Error"].GetType() == typeof(string))
            {
                return RedirectToAction("Index");
            }
            TempData["Error"] = null;
            return View("MessageLog", context.ChatMessages.Where(m => m.Log.DateCreated == model.DateCreated));
        }

		#region DELETE
        [ActionName("ChatLogDelete")]
        public ActionResult Delete(ChatLog model)
        {
			TempData["Error"] = context.Delete(model);
            return RedirectToAction("Index");
        }
        [ActionName("ChatMessageDelete")]
        public ActionResult Delete(ChatMessage model)
        {
            TempData["Error"] = context.Delete(model);
            return RedirectToAction("MessageLog", context.ChatLogs.Where(m => m.DateCreated == model.Log.DateCreated));
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