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
    [Authorize]
    public class GameRoomController : Controller
    {
        private static List<GameViewModel> activeGames = new List<GameViewModel>();

		[HttpGet]
        public ActionResult Index()
        {
            return View(activeGames);
        }

		#region CREATE
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GameViewModel model)
        {
            if (ModelState.IsValid)
            {
                activeGames.Add(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }
		#endregion CREATE

        #region EDIT/UPDATE
        [HttpGet]
        public ActionResult Edit(GameViewModel model)
        {
            if (TempData["Error"].GetType() == typeof(string))
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult PostEdit(GameViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
			// Process model errors.
            return View("Edit", model);
        }
		#endregion EDIT/UPDATE

		#region DELETE
        [HttpPost]
        public ActionResult Delete(GameViewModel model)
        {
            return RedirectToAction("Index");
        }
		#endregion DELETE
    }
}