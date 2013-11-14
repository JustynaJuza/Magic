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

        public GameController() { }

        public ActionResult Index()
        {
            var userId =  User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            if (currentUser != null){
                if (players.Count == players.Capacity)
                {
                    TempData["Message"] = "You have joined the game as an observer, because all player spots have been taken.";
                }
                else if (currentUser.DeckCollection.Count == 0)
                {
                    lock (players)
                    {
                    players.Add(new Player(currentUser));   
                    }
                    TempData["Message"] = "Please select a deck to play with.";
                    RedirectToAction("SelectDeck");
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
            else {
                TempData["Message"] = "You must be logged in to join the game";
            }
            return View();
        }

        public ActionResult SelectDeck(CardDeckViewModel actionItem)
        {
            var userId =  User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            var player = new Player(currentUser);
            player.SelectDeck(actionItem);

            var selectedDeck = currentUser.DeckCollection.FirstOrDefault(d => d.Id == actionItem.Id);
            if (selectedDeck == null)
            {
                selectedDeck = context.Set<CardDeck>().AsNoTracking().FirstOrDefault(d => d.Id == actionItem.Id);
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
	}
}