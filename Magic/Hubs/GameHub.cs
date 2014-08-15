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
                //DisplayPlayerReady(player.User, gameId, isReady);
                player.Status = isReady ? PlayerStatus.Ready : PlayerStatus.Unready;
                context.InsertOrUpdate(player, true);
                Clients.Group(gameId).togglePlayerReady(player.User.UserName, (isReady ? player.User.ColorCode : ""));

                var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                chatHubContext.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), player.User.UserName, player.User.ColorCode,
                    (isReady ? " is ready for action." : " seems to be not prepared!"));

                if (!isReady || (player.Game.Players.Count(p => p.Player.Status == PlayerStatus.Ready) != player.Game.PlayerCapacity)) return;
            }

            // TODO: START THE GAME!
            System.Diagnostics.Debug.WriteLine("LET THE GAMES BEGIN!");
            Clients.Group(gameId).activateGame();
        }

        #region GAME DISPLAY UPDATES
        //public static void DisplayPlayerReady(User user, string gameId, bool isReady)
        //{
        //    var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

        //    if (isReady)
        //    {
        //        // Update display to show player is ready.
        //        gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName, user.ColorCode);
        //        ChatHub.UserStatusUpdate(user.Id, UserStatus.Ready, gameId);
        //    }
        //    else
        //    {
        //        // Update display to show player is not yet ready.
        //        ChatHub.UserStatusUpdate(user.Id, UserStatus.Unready, gameId);
        //        gameHubContext.Clients.Group(gameId).togglePlayerReady(user.UserName);
        //    }
        //}

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

        public void PauseGame(string gameId, bool isPlayerMissing = false)
        {
            using (var context = new MagicDbContext())
            {
                var game = context.Games.Find(gameId);
                if (!game.DateStarted.HasValue) return;

                foreach (var player in game.Players)
                {
                    player.Status = GameStatus.Unfinished;
                }

                game.TimePlayed = game.TimePlayed + (DateTime.Now - (DateTime)(game.DateSuspended.HasValue ? game.DateSuspended : game.DateStarted));
                game.DateSuspended = DateTime.Now;
                context.InsertOrUpdate(game, true);

                if (isPlayerMissing) return;

                var chatRoom = context.ChatRooms.Find(gameId);
                var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                chatHubContext.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), chatRoom.Name, chatRoom.TabColorCode, " the game has been paused.");
            }
        }

        public static void ResetReadyStatus(string gameId)
        {
            using (var context = new MagicDbContext())
            {
                var game = context.Games.Find(gameId);
                foreach (var player in game.Players.Select(p => p.Player))
                {
                    player.Status = PlayerStatus.Unready;
                }
                context.InsertOrUpdate(game, true);
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
            gameHubContext.Groups.Remove(connection.Id, connection.GameId);

            using (var context = new MagicDbContext())
            {
                var game = context.Games.Find(connection.GameId);
                if (game.Players.All(p => p.UserId != connection.UserId))
                {
                    // Remove observer who left.
                    game.Observers.Remove(game.Observers.First(o => o.Id == connection.UserId));
                    return;
                }

                if (game.DateStarted.HasValue && !game.DateEnded.HasValue)
                {
                    // TODO: STOP THE GAME, A PLAYER IS MISSING! Ask players to start game timeout?
                    var player = context.Players.Find(connection.UserId, connection.GameId);
                    player.Status = PlayerStatus.Missing;
                    context.InsertOrUpdate(player, true);

                    var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    chatHubContext.Clients.Group(connection.GameId).addMessage(connection.GameId, DateTime.Now.ToString("HH:mm:ss"), connection.User.UserName, connection.User.ColorCode,
                        " has fled the battle, the game will be interrupted.");
                }
                else
                {
                    var playerStatus = context.GameStatuses.Find(connection.UserId, connection.GameId);
                    context.Delete(playerStatus, true);
                    ResetReadyStatus(game.Id);
                }
            }
        }
        #endregion MANAGE GAME GROUPS
    }
}