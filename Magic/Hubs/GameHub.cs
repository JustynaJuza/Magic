using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Magic.Models.DataContext;
using Magic.Models;
using Magic.Controllers;

namespace Magic.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private static MagicDBContext context = new MagicDBContext();

        public void TogglePlayerReady(string gameId = "")
        {
            bool isReady = gameId != "";

            var userId = Context.User.Identity.GetUserId();
            var foundUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            if (isReady)
            {
                var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameId);
                var foundPlayer = game.Players.FirstOrDefault(p => p.User.Id == userId);
                foundPlayer.ConnectionId = Context.ConnectionId;

                DisplayPlayerReady(foundUser, gameId, isReady);

                if (game.Players.FindAll(p => p.ConnectionId != null).Count == game.PlayerCapacity)
                {
                    // TODO: START THE GAME!
                    System.Diagnostics.Debug.WriteLine("LET THE GAMES BEGIN!");
                    Clients.Group(gameId).activateGame();
                }
            }
            else
            {
                var gameConnection = context.Set<ApplicationUserConnection>().AsNoTracking().FirstOrDefault(c => c.Id == Context.ConnectionId);
                var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameConnection.GameId);
                var foundPlayer = game.Players.FirstOrDefault(p => p.User.Id == userId);
                foundPlayer.ConnectionId = null;

                DisplayPlayerReady(foundUser, gameConnection.GameId, isReady);
            }
        }

        #region GAME DISPLAY UPDATES
        public static void DisplayPlayerReady(ApplicationUser user, string gameId, bool isReady, bool broadcastMessage = true)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            if (isReady)
            {
                gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName, user.ColorCode);
                ChatHub.UserStatusBroadcast(user.Id, UserStatus.Ready, gameId);
            }
            else
            {
                if (broadcastMessage)
                {
                    ChatHub.UserStatusBroadcast(user.Id, UserStatus.Unready, gameId);
                    gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName);
                }
                else
                {
                    gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName, null, true);
                }
            }
        }

        public static void DisplayPlayerJoined(string userName, string gameId)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Clients.Group(gameId).playerJoined(userName);
        }

        public static void DisplayObserverJoined(string userName, string gameId)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Clients.Group(gameId).observerJoined(userName);
        }

        public static void DisplayUserLeft(ApplicationUserConnection gameConnection)
        {

            var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameConnection.GameId);
            var foundPlayer = game.Players.RemoveAll(p => p.User.Id == gameConnection.User.Id);
            if (foundPlayer != 0)
            {
                if (game.DateStarted == null)
                {
                    foreach (var player in game.Players)
                    {
                        GameHub.DisplayPlayerReady(player.User, game.Id, false, false);
                    }
                }
                else
                {
                    // TODO: STOP THE GAME, A PLAYER IS MISSING!
                }
            }
            game.Observers.RemoveAll(o => o.Id == gameConnection.User.Id);

            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Clients.Group(gameConnection.GameId).userLeft(gameConnection.User.UserName);
        }
        #endregion GAME DISPLAY UPDATES

        #region GROUPS
        public static Task JoinGame(string connectionId, string gameId)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            return gameHubContext.Groups.Add(connectionId, gameId);
        }

        public static Task LeaveGame(string connectionId, string gameId)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            return gameHubContext.Groups.Remove(connectionId, gameId);
        }
        #endregion GROUPS
    }
}