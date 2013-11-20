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
        private static List<Player> players = new List<Player>(2);
        private static List<ApplicationUser> observers = new List<ApplicationUser>();

        // Constructor with predefined player list.
        //public GameController(IList<string> playerIdList = null)
        //{
        //    if (playerIdList != null)
        //    {
        //        foreach (var playerId in playerIdList)
        //        {
        //            var foundPlayer = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == playerId);
        //            players.Add(new Player(foundPlayer));
        //        }
        //    }
        //}

        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            if (players.Count != players.Capacity)
            {
                if (!players.Any(p => p.User.Id == currentUser.Id))
                {
                    if (currentUser.DeckCollection.Count == 0)
                    {
                        lock (players)
                        {
                            players.Add(new Player(currentUser));
                        }
                        TempData["Message"] = "Please select a deck to play with before starting the game.";
                        ViewBag.SelectDeck = true;
                    }
                    else
                    {
                        // Play with last used deck.
                        lock (players)
                        {
                            players.Add(new Player(currentUser, currentUser.DeckCollection.ElementAt(0)));
                        }
                    }
                }
            }
            else
            {
                if (!observers.Any(o => o.Id == currentUser.Id))
                {
                    lock (observers)
                    {
                        observers.Add(currentUser);
                    }
                    TempData["Message"] = "You have joined the game as an observer, because all player spots have been taken."
                                        + "You can join by refreshing the page if a spot becomes available.";
                }
            }

            return View();
        }

        public ActionResult SelectDeck(CardDeckViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            var player = new Player(currentUser);
            player.SelectDeck(model);

            var selectedDeck = currentUser.DeckCollection.FirstOrDefault(d => d.Id == model.Id);
            if (selectedDeck == null)
            {
                selectedDeck = context.Set<CardDeck>().AsNoTracking().FirstOrDefault(d => d.Id == model.Id);
                currentUser.DeckCollection.Insert(0, selectedDeck);
                context.Update(currentUser);
            }
            else
            {
                currentUser.DeckCollection.Remove(selectedDeck);
                currentUser.DeckCollection.Insert(0, selectedDeck);
            }

            return View("Index");
        }

        public ActionResult Start()
        {
            UpdateUserStatuses();

   
            return View("Index");
        }

        #region HELPERS
        private void UpdateUserStatuses()
        {
            foreach (var user in observers)
            {
                user.Status = UserStatus.Observing; ;
                context.Update(user);
            }
            foreach (var user in players)
            {
                user.User.Status = UserStatus.Playing;
                context.Update(user.User);
            }
        }
        #endregion HELPERS
    }
}