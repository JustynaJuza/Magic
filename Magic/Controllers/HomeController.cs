using Magic.Models;
using Magic.Models.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Magic.Controllers
{
    public class HomeController : Controller
    {
        private MagicDBContext context = new MagicDBContext();

        public ActionResult Index()
        {
            // TODO: Filter private messages.
            ChatLog currentLog = (ChatLog) HttpContext.ApplicationInstance.Context.Application["GeneralChatLog"];
            if (currentLog.MessageLog.Count > 10)
            {
                currentLog.MessageLog = currentLog.MessageLog.GetRange(currentLog.MessageLog.Count - 10, 10); //Where(m => (m.TimeSend - DateTime.Now) < new TimeSpan(0, 1, 0)).ToList();
            }

            return View(currentLog);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}