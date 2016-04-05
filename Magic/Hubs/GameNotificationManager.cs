using Microsoft.AspNet.SignalR;
using Magic.Models;

namespace Magic.Hubs
{
    public interface IGameNotificationManager
    {
        void DisplayPause(User user, string gameId);
        void TogglePlayerReady(string gameId, bool isReady);
        void DisplayPlayerReady(User user, string gameId, bool isReady);
        void DisplayUserJoined(string userName, string gameId, bool isPlayer);
        void DisplayUserLeft(string userName, string gameId);
        void ResetReadyStatus(string gameId);
    }

    public class GameNotificationManager : IGameNotificationManager
    {
        private readonly IGameHub _gameHub;

        public GameNotificationManager(IGameHub gameHub)
        {
            _gameHub = gameHub;
        }

        public void TogglePlayerReady(string gameId, bool isReady)
        {
            //var userId = Context.User.Identity.GetUserId();

            //var player = _context.Query<Player>().Find(userId, gameId);
            ////DisplayPlayerReady(player.User, gameId, isReady);
            //player.Status = isReady ? PlayerStatus.Ready : PlayerStatus.Unready;
            //_context.InsertOrUpdate(player, withSave: true, updateOnly: true);
            //Clients.Group(gameId).togglePlayerReady(player.User.UserName, (isReady ? player.User.ColorCode : ""));

            //var chatHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            //chatHubContext.Clients.Group(gameId).addMessage(gameId, DateTime.Now.ToString("HH:mm:ss"), player.User.UserName, player.User.ColorCode,
            //    (isReady ? " is ready for action." : " seems to be not prepared!"));

            //if (!isReady || (player.Game.Players.Count(p => p.Player.Status == PlayerStatus.Ready) != player.Game.PlayerCapacity)) return;

            //StartGame(gameId);
        }

        public void DisplayPause(User user, string gameId)
        {
            _gameHub.Clients.Group(gameId).pauseGame("has paused the game.", user.UserName, user.ColorCode);
        }

        public void DisplayPlayerReady(User user, string gameId, bool isReady)
        {
            _gameHub.Clients.Group(gameId).togglePlayerReady(user.UserName, user.ColorCode);
        }

        public void DisplayUserJoined(string userName, string gameId, bool isPlayer)
        {
            if (isPlayer)
            {
                _gameHub.Clients.Group(gameId).playerJoined(userName);
            }
            else
            {
                _gameHub.Clients.Group(gameId).observerJoined(userName);
            }
        }

        public void DisplayUserLeft(string userName, string gameId)
        {
            _gameHub.Clients.Group(gameId).userLeft(userName);
        }

        public void ResetReadyStatus(string gameId)
        {
            _gameHub.Clients.Group(gameId).resetReadyStatus();
        }
    }
}