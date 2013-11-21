using Magic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Magic.Models.DataContext;
using Magic.Hubs;

namespace Magic.Controllers
{
    public class GameController : Controller
    {
        private MagicDBContext context = new MagicDBContext();
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
        public ActionResult Index(GameViewModel game)
        {
            Session["Game"] = game;

            var userId = User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            if (game.Players.Count < game.PlayerCount)
            {
                if (!game.Players.Any(p => p.User.Id == currentUser.Id))
                {
                    if (currentUser.DeckCollection.Count == 0)
                    {
                        lock (game.Players)
                        {
                            game.Players.Add(new Player(currentUser));
                        }
                        TempData["Message"] = "Please select a deck to play with before starting the game.";
                        ViewBag.SelectDeck = true;
                    }
                    else
                    {
                        // Initialise with last used deck.
                        lock (game.Players)
                        {
                            game.Players.Add(new Player(currentUser, currentUser.DeckCollection.ElementAt(0)));
                        }
                    }
                }
            }
            else
            {
                if (!game.Observers.Any(o => o.Id == currentUser.Id))
                {
                    lock (game.Observers)
                    {
                        game.Observers.Add(currentUser);
                    }
                    TempData["Message"] = "You have joined the game as an observer, because all player spots have been taken.\n"
                                        + "You can take a player seat by refreshing the page if a spot becomes available.";
                }
            }

            // Join game room chat.
            ChatHub.ActivateGameChat(currentUser.Id, game.Id);


            var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.ChatHub>();
            hubContext.Clients.Group(game.Id).addMessage(DateTime.Now.ToString("HH:mm:ss"), "Server", "#000000", "joined");
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
            var game = (GameViewModel) Session["Game"];
            UpdateUserStatuses(game);

            foreach (var player in game.Players)
            {
                GameHub.ActivateGame(player.User.Id, game.Id);
            }

            return View("Index");
        }

        #region HELPERS
        private void UpdateUserStatuses(GameViewModel game)
        {
            if (game.Observers != null) { 
            foreach (var user in game.Observers)
            {
                user.Status = UserStatus.Observing; ;
                context.Update(user);
            }}
            foreach (var user in game.Players)
            {
                user.User.Status = UserStatus.Playing;
                context.Update(user.User);
            }
        }
        #endregion HELPERS
    }
}