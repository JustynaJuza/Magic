using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Magic.Hubs;
using Magic.Models;
using Newtonsoft.Json;

namespace Magic.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : AsyncController
    {
        public ActionResult Index()
        {
            return View();
        }

        [AsyncTimeout(200)]
        public async Task<ActionResult> GetCardsAsync()
        {
            var card = await AdminHub.MakeCardsRequest();
            return View(card);
        }
    }
}