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
        public List<string> GetPlayerGameConnections(string gameId)
        {
            var userId = Context.User.Identity.GetUserId();
            using (var context = new MagicDbContext())
            {
                return context.Connections.Where(c => c.UserId == userId && c.GameId == gameId).Select(c => c.Id).ToList();
            }
        }

        public void TogglePlayerReady(string gameId, bool isReady)
        {
            var userId = Context.User.Identity.GetUserId();

            var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameId);
            var player = game.Players.FirstOrDefault(p => p.User.Id == userId);
            DisplayPlayerReady(player.User, gameId, isReady);

            if (!isReady || (game.Players.Count(p => p.User.Status == UserStatus.Ready) != game.PlayerCapacity)) return;

            // TODO: START THE GAME!
            System.Diagnostics.Debug.WriteLine("LET THE GAMES BEGIN!");
            Clients.Group(gameId).activateGame();
        }

        #region GAME DISPLAY UPDATES
        public static void DisplayPlayerReady(User user, string gameId, bool isReady, bool broadcastMessage = true)
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

        public void ResetReadyStatus(string gameId)
        {
            var players = GameRoomController.activeGames.Find(g => g.Id == gameId).Players.Select(p => p.User);
            foreach (var player in players)
            {
                ChatHub.UserStatusBroadcast(player.Id, player.Status = UserStatus.Unready, gameId);
            }
            Clients.Group(gameId).resetReadyStatus();
        }

        public async Task JoinGame(string gameId, string userName, bool isPlayer)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            var joinGame = gameHubContext.Groups.Add(Context.ConnectionId, gameId);

            if (isPlayer)
            {
                gameHubContext.Groups.Add(Context.ConnectionId, gameId + "_players");
                DisplayPlayerJoined(userName, gameId);
                ResetReadyStatus(gameId);
            }
            else
            {
                DisplayObserverJoined(userName, gameId);
            }

            await joinGame;
        }

        public static Task LeaveGame(UserConnection connection)
        {
            var game = GameRoomController.activeGames.Find(g => g.Id == connection.GameId);
            var wasPlayer = game.Players.Remove(game.Players.First(p => p.User.Id == connection.UserId));

            // Update game accordingly if a player left.
            if (wasPlayer)
            {
                if (!game.DateStarted.HasValue)
                {
                    // Set all other player to 'not ready'.
                    foreach (var player in game.Players)
                    {
                        //player.ConnectionId = null;
                        DisplayPlayerReady(player.User, game.Id, false, false);
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
                game.Observers.Remove(game.Observers.First(o => o.Id == connection.UserId));
            }

            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            return gameHubContext.Groups.Remove(connection.Id, connection.GameId);
        }
        #endregion MANAGE GAME GROUPS
    }
}