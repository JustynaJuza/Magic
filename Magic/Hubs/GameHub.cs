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
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();
            var isReady = gameId.Length > 0;

            if (isReady)
            {
                // Set the player's game connection for future game calls.
                var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameId);
                var foundPlayer = game.Players.FirstOrDefault(p => p.User.Id == userId);
                foundPlayer.ConnectionId = Context.ConnectionId;

                GameHub.DisplayPlayerReady(foundPlayer.User, gameId, isReady);

                // Start the game if all players are ready.
                if (game.Players.FindAll(p => p.ConnectionId != null).Count == game.PlayerCapacity)
                {
                    // TODO: START THE GAME!
                    System.Diagnostics.Debug.WriteLine("LET THE GAMES BEGIN!");
                    Clients.Group(gameId).activateGame();
                }
            }
            else
            {
                // Reset the game connection data when the player is unready to play.
                var gameConnection = context.Set<ApplicationUserGameConnection>().AsNoTracking().FirstOrDefault(c => c.Id == Context.ConnectionId);
                var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameConnection.ChatRoom.Id);
                var foundPlayer = game.Players.FirstOrDefault(p => p.User.Id == userId);
                foundPlayer.ConnectionId = null;

                GameHub.DisplayPlayerReady(foundPlayer.User, game.Id, isReady);
            }
        }

        #region GAME DISPLAY UPDATES
        public static void DisplayPlayerReady(ApplicationUser user, string gameId, bool isReady, bool broadcastMessage = true)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

            if (isReady)
            {
                // Update display to show player is ready.
                gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName, user.ColorCode);
                ChatHub.UserStatusBroadcast(user.Id, UserStatus.Ready, gameId);
            }
            else
            {
                if (broadcastMessage)
                {
                    // Update display to show player is not yet ready.
                    ChatHub.UserStatusBroadcast(user.Id, UserStatus.Unready, gameId);
                    gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName);
                }
                else
                {
                    // Automatically update display without message if other player leaves game before start.
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

        public static void DisplayUserLeft(string userName, string gameId)
        {
            // Update display and remove the user who left.
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Clients.Group(gameId).userLeft(userName);
        }
        #endregion GAME DISPLAY UPDATES

        #region MANAGE GAME GROUPS
        public static Task JoinGame(string connectionId, string gameId)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            return gameHubContext.Groups.Add(connectionId, gameId);
        }

        public static Task LeaveGame(ApplicationUserGameConnection gameConnection)
        {
            var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameConnection.ChatRoom.Id);
            var foundPlayer = game.Players.RemoveAll(p => p.User.Id == gameConnection.User.Id);

            // Update game accordingly if a player left.
            if (foundPlayer != 0)
            {
                if (game.DateStarted == null)
                {
                    // Set all other player to 'not ready'.
                    foreach (var player in game.Players)
                    {
                        player.ConnectionId = null;
                        GameHub.DisplayPlayerReady(player.User, game.Id, false, false);
                    }
                }
                else
                {
                    // TODO: STOP THE GAME, A PLAYER IS MISSING!
                }
            }
            else
            {
                // Remove observer who left.
                game.Observers.RemoveAll(o => o.Id == gameConnection.User.Id);
            }

            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            return gameHubContext.Groups.Remove(gameConnection.Id, gameConnection.Game.Id);
        }
        #endregion MANAGE GAME GROUPS
    }
}