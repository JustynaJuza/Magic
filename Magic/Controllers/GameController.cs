using System.Threading;
using System.Threading.Tasks;
using Magic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Magic.Models.DataContext;
using Magic.Hubs;
using Microsoft.AspNet.SignalR;

namespace Magic.Controllers
{
    [System.Web.Mvc.Authorize]
    public class GameController : Controller
    {
        public static int DefaultPlayerHealth = 20;
        private MagicDbContext context = new MagicDbContext();
        // Constructor with predefined player list.
        //public GameController(IList<string> playerIdList = null)
        //{
        //    if (playerIdList != null)
        //    {
        //        foreach (var playerId in playerIdList)
        //        {
        //            var foundPlayer = context.Set<User>().AsNoTracking().FirstOrDefault(u => u.Id == playerId);
        //            players.Add(new Player(foundPlayer));
        //        }
        //    }
        //}

        [HttpGet]
        public ActionResult Index(string gameId)
        {
            var game = context.Games.Find(gameId);
            if (game == null)
            {
                TempData["Error"] = "The game you were looking for is no longer in progress. Maybe it finished without you or timed out.";
                return RedirectToAction("Index", "GameRoom");
            }

            var userId = User.Identity.GetUserId();
            var isPlayer = game.Players.Any(p => p.User.Id == userId);
            if (game.IsPrivate && !isPlayer)
            {
                TempData["Error"] = "You are not allowed to join this private game. You can message the room owner to ask for an invitation.";
                return RedirectToAction("Index", "GameRoom");
            }

            if (isPlayer)
            {
                ViewBag.IsPlayer = true;
                // TODO: choose deck!
                return View((GameViewModel)game.GetViewModel());
            }

            Session["GameId"] = gameId;
            var user = context.Users.Find(userId);

            lock (game.Players)
            {
                if (game.Players.Count < game.PlayerCapacity)
                {
                    game.Players.Add(new GamePlayerStatus(new Player
                    {
                        GameId = gameId,
                        UserId = userId,
                        User = user
                    }));
                    context.InsertOrUpdate(game);

                    ViewBag.IsPlayer = true;
                    return View((GameViewModel)game.GetViewModel());
                }
            }

            if (game.Observers.All(o => o.Id != userId))
            {
                game.Observers.Add(user.GetViewModel());
                TempData["Message"] = "You have joined the game as an observer, because all player spots have been taken.\n"
                                    + "You can take a player seat by refreshing the page if a spot becomes available.";
            }

            ViewBag.IsPlayer = false;
            return View((GameViewModel)game.GetViewModel());

            //if (currentUser.DeckCollection.Any())
            //{
            //    // Initialise with last used deck.
            //    lock (game.Players)
            //    {
            //        game.Players.Add(new Player(currentUser, currentUser.DeckCollection.ElementAt(0)));
            //    }
            //}
            //else
            //{
            //    lock (game.Players)
            //    {
            //        game.Players.Add(new Player(currentUser));
            //    }
            //    TempData["Message"] = "Please select a deck to play with before starting the game.";
            //    ViewBag.SelectDeck = true;
            //}
        }

        //public ActionResult SelectDeck(CardDeckViewModel model)
        //{
        //    var userId = User.Identity.GetUserId();
        //    var currentUser = context.Set<User>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

        //    var player = new Player(currentUser);
        //    player.SelectDeck(model);

        //    var selectedDeck = currentUser.DeckCollection.FirstOrDefault(d => d.Id == model.Id);
        //    if (selectedDeck == null)
        //    {
        //        selectedDeck = context.Set<CardDeck>().AsNoTracking().FirstOrDefault(d => d.Id == model.Id);
        //        currentUser.DeckCollection.Insert(0, selectedDeck);
        //        context.InsertOrUpdate(currentUser);
        //    }
        //    else
        //    {
        //        currentUser.DeckCollection.Remove(selectedDeck);
        //        currentUser.DeckCollection.Insert(0, selectedDeck);
        //    }

        //    return View("Index");
        //}

        //public ActionResult Start()
        //{
        //    var game = GameRoomController.ActiveGames.FirstOrDefault(g => g.Id == (string)Session["GameId"]);
        //    UpdateUserStatuses(game);

        //    var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.GameHub>();
        //    hubContext.Clients.Group(game.Id).activateGame();

        //    foreach (var player in game.Players)
        //    {
        //        //GameHub.ActivateGameForPlayer(player.User.Id, game.Id);
        //    }

        //    return View("Index");
        //}

        public async Task Pause(string gameId)
        {
            var dateSuspended = DateTime.Now;
            HttpContext.Application[gameId] = new CancellationTokenSource();
            var user = context.Users.Find(User.Identity.GetUserId());
            await GameHub.PauseGame(user, gameId, dateSuspended, ((CancellationTokenSource)HttpContext.Application[gameId]).Token);
        }

        public void CancelPause(string gameId)
        {
            ((CancellationTokenSource)HttpContext.Application[gameId]).Cancel();
        }

        #region HELPERS
        private void UpdateUserStatuses(Game game)
        {
            if (game.Observers != null)
            {
                foreach (var observer in game.Observers)
                {
                    var user = context.Users.Find(observer.Id);
                    user.Status = UserStatus.Observing;
                    context.InsertOrUpdate(user, true);
                }
            }
            foreach (var player in game.Players)
            {
                player.User.Status = UserStatus.Playing;
                player.User.Games.Add(new GamePlayerStatus()
                {
                    GameId = game.Id,
                    UserId = player.User.Id,
                    User = player.User,
                    Status = GameStatus.InProgress
                });
                context.InsertOrUpdate(player.User);
            }
        }
        #endregion HELPERS
    }
}