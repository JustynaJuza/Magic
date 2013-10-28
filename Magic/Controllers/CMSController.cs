using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magic.Controllers
{
    public class CMSController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}