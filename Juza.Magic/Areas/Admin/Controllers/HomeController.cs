using System.Web.Mvc;

namespace Juza.Magic.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //[AsyncTimeout(200)]
    }
}