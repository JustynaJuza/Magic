using Magic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Magic.Models.DataContext;
using Magic.Hubs;
using Microsoft.AspNet.SignalR;

namespace Magic.Controllers
{
    [System.Web.Mvc.Authorize]
    public class GameController : Controller
    {
        private MagicDbContext context = new MagicDbContext();
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

        [HttpGet]
        public ActionResult Index(string gameId)
        {
            Session["GameId"] = gameId;
            var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
            {
                TempData["Error"] = "The game you were looking for is no longer in progress. Maybe it finished without you or timed out.";
                return RedirectToAction("Index", "GameRoom");
            }

            var userId = User.Identity.GetUserId();
            var currentUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            if (!game.Players.Any(p => p.User.Id == currentUser.Id))
            {
                if (game.Players.Count < game.PlayerCapacity)
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

                    ViewBag.IsPlayer = true;
                    GameHub.DisplayPlayerJoined(currentUser.UserName, game.Id);
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

                    ViewBag.IsPlayer = false;
                    GameHub.DisplayObserverJoined(currentUser.UserName, game.Id);
                }
            }
            else
            {
                ViewBag.IsPlayer = true;
            }

            // Join game room chat.
            //ChatHub.ToggleGameChatSubscription(gameId, true);
            return View(game);
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
                context.InsertOrUpdate(currentUser);
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
            var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == (string) Session["GameId"]);
            UpdateUserStatuses(game);

                var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.GameHub>();
                hubContext.Clients.Group(game.Id).activateGame();

            foreach (var player in game.Players)
            {
                //GameHub.ActivateGameForPlayer(player.User.Id, game.Id);
            }

            return View("Index");
        }

        #region HELPERS
        private void UpdateUserStatuses(GameViewModel game)
        {
            if (game.Observers != null) { 
            foreach (var user in game.Observers)
            {
                user.Games.Add(new Player_GameStatus()
                {
                    GameId = game.Id,
                    UserId = user.Id,
                    User = user,
                    Status = GameStatus.Observed
                });
                user.Status = UserStatus.Observing; ;
                context.InsertOrUpdate(user);
            }}
            foreach (var user in game.Players)
            {
                user.User.Status = UserStatus.Playing;
                user.User.Games.Add(new Player_GameStatus()
                {
                    GameId = game.Id,
                    UserId = user.User.Id,
                    User = user.User,
                    Status = GameStatus.Unfinished
                });
                context.InsertOrUpdate(user.User);
            }
        }
        #endregion HELPERS
    }
}