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
        public static List<GameViewModel> activeGames = new List<GameViewModel>();

        [HttpGet]
        public ActionResult Index()
        {
            return View(activeGames);
        }

        public ActionResult Create()
        {
            GameViewModel game = new GameViewModel();
            activeGames.Add(game);
            return RedirectToAction("Index", "Game", new { gameId = game.Id });
        }

        public ActionResult Join(string Id)
        {
            var game = activeGames.FirstOrDefault(g => g.Id == Id);
            if (game == null)
            {
                TempData["Error"] = "This game is already finished or no longer available.";
            }
            return RedirectToAction("Index", "Game", new { gameId = Id });
        }

    }
}