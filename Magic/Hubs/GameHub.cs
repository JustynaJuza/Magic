using System.Threading;
using Magic.Models.Chat;
using Magic.Models.Extensions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Magic.Models.DataContext;
using Magic.Models;
using Microsoft.AspNet.SignalR.Hubs;

namespace Magic.Hubs
{
    public interface IGameHub
    {
        List<string> GetPlayerGameConnections(string gameId);
        void TogglePlayerReady(string gameId, bool isReady);
        Task PauseGame(User user, string gameId, DateTime dateSuspended, CancellationToken token);
        void StartGame(string gameId);
        void ResetReadyStatus(string gameId);
        Task JoinGame(string gameId, string userName, bool isPlayer);
        Task LeaveGame(UserConnection connection);
        Task OnDisconnected(bool stopCalled);
        Task OnConnected();
        Task OnReconnected();
        void Dispose();
        IHubCallerConnectionContext<dynamic> Clients { get; set; }
        HubCallerContext Context { get; set; }
        IGroupManager Groups { get; set; }
    }

    [Authorize]
    public class GameHub : Hub, IGameHub
    {
        private readonly IDbContext _context;

        public GameHub(IDbContext context)
        {
            _context = context;
        }

        public List<string> GetPlayerGameConnections(string gameId)
        {
            var userId = Context.User.Identity.GetUserId();
            return _context.Query<UserConnection>().Where(c => c.UserId == userId && c.GameId == gameId).Select(c => c.Id).ToList();
        }

        public void TogglePlayerReady(string gameId, bool isReady)
        {
            var userId = Context.User.Identity.GetUserId();

            var player = _context.Query<Player>().Find(userId, gameId);
            //DisplayPlayerReady(player.User, gameId, isReady);
            player.Status = isReady ? PlayerStatus.Ready : PlayerStatus.Unready;
            _context.InsertOrUpdate(player, withSave: true, updateOnly: true);
            Clients.Group(gameId).togglePlayerReady(player.User.UserName, (isReady ? player.User.ColorCode : ""));

            var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            chatHubContext.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), player.User.UserName, player.User.ColorCode,
                (isReady ? " is ready for action." : " seems to be not prepared!"));

            if (!isReady || (player.Game.Players.Count(p => p.Player.Status == PlayerStatus.Ready) != player.Game.PlayerCapacity)) return;

            StartGame(gameId);
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

        public async Task PauseGame(User user, string gameId, DateTime dateSuspended, CancellationToken token)
        {
            using (var context = new MagicDbContext())
            {
                var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
                gameHubContext.Clients.Group(gameId).pauseGame("has paused the game.", user.UserName, user.ColorCode);
                var pause = Task.Delay(10000, token);

                //var chatRoom = _context.Query<ChatRoom>().Find(gameId);
                var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                chatHubContext.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), user.UserName, user.ColorCode, "has paused the game.");

                var game = _context.Query<Game>().Find(gameId);
                game.UpdateTimePlayed(dateSuspended);

                try
                {
                    await pause;
                }
                catch (OperationCanceledException) { }
                finally
                {
                    game.DateResumed = DateTime.Now;
                    gameHubContext.Clients.Group(gameId).activateGame(game.TimePlayed.ToTotalHoursString());
                    context.InsertOrUpdate(game, true);
                }
            }
        }

        public void StartGame(string gameId)
        {
            System.Diagnostics.Debug.WriteLine("LET THE GAMES BEGIN!");
            using (var context = new MagicDbContext())
            {
                var game = context.Read<Game>().FindOrFetchEntity(gameId);
                if (game.DateStarted.HasValue)
                {
                    game.DateResumed = DateTime.Now;
                }
                else
                {
                    game.DateStarted = DateTime.Now;
                }

                foreach (var player in game.Players)
                {
                    player.Status = GameStatus.InProgress;
                }
                _context.InsertOrUpdate(game, withSave: true, updateOnly: true);

                Clients.Group(gameId).activateGame();
            }
        }

        public void ResetReadyStatus(string gameId)
        {
            var game = _context.Query<Game>().Find(gameId);
            foreach (var player in game.Players.Select(p => p.Player))
            {
                player.Status = PlayerStatus.Unready;
            }
            _context.InsertOrUpdate(game, withSave: true, updateOnly: true);

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

            var addPlayerToGame = Models.Extensions.TaskExtensions.CompletedTask;
            if (isPlayer)
            {
                addPlayerToGame = gameHubContext.Groups.Add(Context.ConnectionId, gameId + "_players");
                ResetReadyStatus(gameId);
            }

            await Task.WhenAll(joinGame, addPlayerToGame);
        }

        public async Task LeaveGame(UserConnection connection)
        {
            var dateSuspended = DateTime.Now;
            var gameHubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            gameHubContext.Groups.Remove(connection.Id, connection.GameId);
            gameHubContext.Clients.Group(connection.GameId).userLeft(connection.User.UserName);

            var game = _context.Query<Game>().Find(connection.GameId);
            if (game.Players.All(p => p.UserId != connection.UserId))
            {
                // Remove observer who left.
                game.Observers.Remove(game.Observers.First(o => o.Id == connection.UserId));
                return;
            }

            if (game.DateStarted.HasValue && !game.DateEnded.HasValue)
            {
                // TODO: STOP THE GAME, A PLAYER IS MISSING! Ask players to start game timeout?
                game.UpdateTimePlayed(dateSuspended);
                game.DateResumed = null;
                gameHubContext.Clients.Group(connection.GameId).pauseGame("has fled the battle!", connection.User.UserName, connection.User.ColorCode);

                var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                chatHubContext.Clients.Group(connection.GameId).addMessage(connection.GameId, DateTime.Now.ToString("HH:mm:ss"), connection.User.UserName, connection.User.ColorCode,
                    "has fled the battle, the game will be interrupted.");

                var playerStatus = game.Players.First(ps => ps.UserId == connection.UserId);
                playerStatus.Status = GameStatus.Unfinished;
                playerStatus.Player.Status = PlayerStatus.Missing;
                _context.InsertOrUpdate(game, withSave: true, updateOnly: true);
            }
            else
            {
                var playerStatus = _context.Query<GamePlayerStatus>().Find(connection.UserId, connection.GameId);
                _context.Delete(game, withSave: true, deleteOnly: true);
                ResetReadyStatus(game.Id);
            }
        }
        #endregion MANAGE GAME GROUPS
    }
}