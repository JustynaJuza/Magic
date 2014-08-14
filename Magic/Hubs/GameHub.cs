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
using System.Data.Entity;

namespace Magic.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        public static List<Game> GetGames()
        {
            using (var context = new MagicDbContext())
            {
                return context.Games.Include(g => g.Players.Select(p => p.Player)).Where(g => g.DateEnded.HasValue == false).ToList();
            }
        }

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

            using (var context = new MagicDbContext())
            {
                var player = context.Players.Find(userId, gameId);
                DisplayPlayerReady(player.User, gameId, isReady);

                if (!isReady || (player.Game.Players.Count(p => p.User.Status == UserStatus.Ready) != player.Game.PlayerCapacity)) return;
            }

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
            var users = GameRoomController.ActiveGames.Find(g => g.Id == gameId).Players.Select(p => p.User);
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

        public async static Task LeaveGame(UserConnection connection)
        {
            DisplayUserLeft(connection.User.UserName, connection.GameId);

            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            var leavingGroup = gameHubContext.Groups.Remove(connection.Id, connection.GameId);

            using (var context = new MagicDbContext())
            {
                var player = context.Players.Find(connection.UserId, connection.GameId);
                var game = player.Game;
                var wasPlayer = context.Delete(player);

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
            }

            await leavingGroup;
        }
        #endregion MANAGE GAME GROUPS
    }
}