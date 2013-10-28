using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magic.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Users="justyna")]
        public ActionResult RestrictedToUser()
        {
            return View();
        }

        [Authorize(Roles="Developer")]
        public ActionResult RestrictedToRole()
        {
            return View();
        }
    }
}