using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Magic.Hubs;
using Magic.Models;
using Magic.Models.DataContext;

namespace Magic.Controllers
{
    [Authorize]
    public class GameRoomController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        [HttpGet]
        public ActionResult Index()
        {
            var activeGames = context.Set<Game>().Include(g => g.Players.Select(p => p.User)).Where(g => g.DateEnded.HasValue == false).ToList();

            return View(activeGames);
        }

        public ActionResult Create()
        {
            var game = new Game();
            context.Insert(game);

            return RedirectToAction("Index", "Game", new { gameId = game.Id });
        }

        public ActionResult Join(string id)
        {
            var game = context.Read<Game>().FindOrFetchEntity(id);
            if (game == null)
            {
                TempData["Error"] = "This game has already finished or is no longer available.";
            }
            return RedirectToAction("Index", "Game", new { gameId = id });
        }

    }
}