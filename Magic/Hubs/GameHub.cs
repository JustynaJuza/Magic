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
        public static void DisplayPlayerReady(User user, string gameId, bool isReady)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

            if (isReady)
            {
                // Update display to show player is ready.
                gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName, user.ColorCode);
                ChatHub.UserStatusUpdate(user.Id, UserStatus.Ready, gameId);
            }
            else
            {
                // Update display to show player is not yet ready.
                ChatHub.UserStatusUpdate(user.Id, UserStatus.Unready, gameId);
                gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName);
            }
        }
        
        public static void DisplayUserJoined(string userName, string gameId, bool isPlayer)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            if (isPlayer)
            {
                gameHubContext.Clients.Group(gameId).playerJoined(userName);
            }
            else
            {
                gameHubContext.Clients.Group(gameId).observerJoined(userName);
            }
        }

        public static void DisplayUserLeft(string userName, string gameId)
        {
            // Update display and remove the user who left.
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Clients.Group(gameId).userLeft(userName);
        }

        public static void ResetReadyStatus(string gameId)
        {
            var users = GameRoomController.activeGames.Find(g => g.Id == gameId).Players.Select(p => p.User);
            using (var context = new MagicDbContext())
            {
                foreach (var user in users)
                {
                    user.Status = UserStatus.Unready;
                    context.InsertOrUpdate(user);
                }
            }

            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Clients.Group(gameId).resetReadyStatus();
        }
        #endregion GAME DISPLAY UPDATES

        #region MANAGE GAME GROUPS
        public async Task JoinGame(string gameId, string userName, bool isPlayer)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            var joinGame = gameHubContext.Groups.Add(Context.ConnectionId, gameId);
            DisplayUserJoined(userName, gameId, isPlayer);

            if (isPlayer)
            {
                gameHubContext.Groups.Add(Context.ConnectionId, gameId + "_players");
                ResetReadyStatus(gameId);
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
                    ResetReadyStatus(game.Id);
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

            DisplayUserLeft(connection.User.UserName, game.Id);
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            return gameHubContext.Groups.Remove(connection.Id, connection.GameId);
        }
        #endregion MANAGE GAME GROUPS
    }
}