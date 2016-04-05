using System.Threading;
using Magic.Models.Extensions;
using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Magic.Models.DataContext;
using Magic.Models;

namespace Magic.Hubs
{
    public interface IGameConnectionManager
    {
        Task PauseGame(User user, string gameId, DateTime dateSuspended, CancellationToken token);
        void StartGame(string gameId);
        void ResetReadyStatus(string gameId);
        Task JoinGame(string gameId, string userName, bool isPlayer);
        Task LeaveGame(UserConnection connection);
    }

    public class GameConnectionManager : IGameConnectionManager
    {
        private readonly IDbContext _context;
        private readonly IGameNotificationManager _gameNotificationManager;
        private readonly IChatNotificationManager _chatNotificationManager;
        private readonly IHubContext _gameHub;
        private readonly IHubContext<ChatHub> _chatHub;

        public GameConnectionManager(
            IDbContext context,
            IGameNotificationManager gameNotificationManager,
            IChatNotificationManager chatNotificationManager,
            IHubContext<IGameHub> gameHub,
            IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _gameNotificationManager = gameNotificationManager;
            _chatNotificationManager = chatNotificationManager;
            _gameHub = gameHub;
            _chatHub = chatHub;
        }

        public async Task PauseGame(User user, string gameId, DateTime dateSuspended, CancellationToken token)
        {
            var pause = Task.Delay(10000, token);

            _gameNotificationManager.DisplayPause(user, gameId);
            _chatNotificationManager.DisplayPause(user, gameId);

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
                ResumeGame(gameId, game.TimePlayed.ToTotalHoursString());
                _context.InsertOrUpdate(game, true);
            }
        }

        public async Task ResumeGame(string gameId, string timePlayed)
        {
            _gameHub.Clients.Group(gameId).activateGame(timePlayed);
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

                _gameHub.Clients.Group(gameId).activateGame();
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

        #region MANAGE GAME GROUPS
        public async Task JoinGame(string gameId, string userName, bool isPlayer)
        {
            var joinGame = _gameHub.Groups.Add(_gameHub.Context.ConnectionId, gameId);
            _gameNotificationManager.DisplayUserJoined(userName, gameId, isPlayer);

            var addPlayerToGame = Models.Extensions.TaskExtensions.CompletedTask;
            if (isPlayer)
            {
                addPlayerToGame = _gameHub.Groups.Add(_gameHub.Context.ConnectionId, gameId + "_players");
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