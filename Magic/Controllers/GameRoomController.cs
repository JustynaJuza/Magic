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
        private MagicDbContext context = new MagicDbContext();
        public static List<GameViewModel> activeGames = new List<GameViewModel>();

        [HttpGet]
        public ActionResult Index()
        {
            return View(activeGames);
        }

        public ActionResult Create()
        {
            var game = new Game();
            context.Insert(game);

            activeGames.Add((GameViewModel) game.GetViewModel());
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