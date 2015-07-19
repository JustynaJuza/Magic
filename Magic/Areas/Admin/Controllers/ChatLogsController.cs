using System;
using System.Linq;
using System.Web.Mvc;
using Magic.Models.Chat;
using Magic.Models.DataContext;

namespace Magic.Areas.Admin.Controllers
{
    public class ChatLogsController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

		[HttpGet]
        public ActionResult Index()
        {
            return View(context.ChatLogs.ToList());
        }

        public ActionResult Messages(string id)
        {
            var model = context.ChatLogs.Find(id);
            if (model != null) return View(model.Messages);

            TempData["Error"] = MagicDbContext.ShowErrorMessage(new ArgumentNullException());
            return RedirectToAction("Index");
        }

		#region DELETE
        [ActionName("ChatLogDelete")]
        public ActionResult Delete(ChatLog model)
        {
            string errorText;
            TempData["Error"] = context.Delete(model, out errorText) ? null : errorText;
            return RedirectToAction("Index");
        }
        [ActionName("ChatMessageDelete")]
        public ActionResult Delete(ChatMessage model)
        {
            string errorText;
            TempData["Error"] = context.Delete(model, out errorText) ? null : errorText;
            return RedirectToAction("Messages", new { id = model.LogId });
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