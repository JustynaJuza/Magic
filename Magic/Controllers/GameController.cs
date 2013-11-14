using Magic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Magic.Models.DataContext;

namespace Magic.Controllers
{
    public class GameController : Controller
    {
        private MagicDBContext context = new MagicDBContext();
        private List<PlayerViewModel> players;

        public GameController() {
            players = new List<PlayerViewModel>();
        }

        public ActionResult Index()
        {
            var currentUserId =  User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == currentUserId);

            if (currentUser != null){
                var player = new PlayerViewModel(currentUser, currentUser.DeckCollection.ElementAt(0));
                players.Add(player);
            }
            else {
                TempData["Message"] = "You must be logged in to join the game";
            }
            return View();
        }
	}
}