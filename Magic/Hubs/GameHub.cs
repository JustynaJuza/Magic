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

        public void ActivateGame(string roomName = "")
        {
            bool joinedGame = roomName != "";

            var userId = Context.User.Identity.GetUserId();
            var foundUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);

            if (joinedGame)
            {
                Groups.Add(Context.ConnectionId, roomName);

                var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == roomName);
                var foundPlayer = game.Players.FirstOrDefault(p => p.User.Id == userId);
                foundPlayer.ConnectionId = Context.ConnectionId;

                Clients.Group(roomName).playerReady(foundUser.UserName, foundUser.ColorCode);
                ChatHub.UserStatusBroadcast(userId, UserStatus.Ready, roomName);

                if (game.Players.All(p => p.ConnectionId != null))
                {
                    // TODO: START THE GAME!
                    System.Diagnostics.Debug.WriteLine("LET THE GAMES BEGIN!");
                    Clients.Group(roomName).activateGame();
                }
            }
            else
            {
                var gameConnection = context.Set<ApplicationUserConnection>().AsNoTracking().FirstOrDefault(c => c.Id == Context.ConnectionId);
                var game = GameRoomController.activeGames.FirstOrDefault(g => g.Id == gameConnection.GameId);
                var foundPlayer = game.Players.FirstOrDefault(p => p.User.Id == userId);
                foundPlayer.ConnectionId = null;

                ChatHub.UserStatusBroadcast(userId, UserStatus.Unready, roomName);
                Clients.Group(roomName).playerNotReady(foundUser.UserName);
                Groups.Remove(Context.ConnectionId, gameConnection.GameId);
            }
        }

        public static void PlayerJoined(string userName, string roomName)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

            gameHubContext.Clients.Group(roomName).playerJoined(userName);
        }

        public static void PlayerLeft(ApplicationUserConnection connection)
        {
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

            gameHubContext.Clients.Group(connection.GameId).playerLeft(connection.User.UserName);
            gameHubContext.Groups.Remove(connection.Id, connection.GameId);
        }

        #region GROUPS
        public static void ActivateGameForPlayer(string userId, string roomName, bool joinedGame = true)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Magic.Hubs.GameHub>();
            hubContext.Clients.Group(roomName).activateGame();
            using (MagicDBContext context = new MagicDBContext())
            {
                var foundUser = context.Set<ApplicationUser>().AsNoTracking().FirstOrDefault(u => u.Id == userId);
                if (joinedGame)
                {
                    foreach (var connection in foundUser.Connections)
                    {
                        hubContext.Groups.Add(connection.Id, roomName);
                    }
                }
                else
                {
                    foreach (var connection in foundUser.Connections)
                    {
                        hubContext.Groups.Remove(connection.Id, roomName);
                    }
                }
            }
        }
        #endregion GROUPS
    }
}