using Magic.Models.Helpers;

namespace Magic.Models
{
    public class GamePlayerStatus : AbstractExtensions
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public virtual User User { get; set; }
        public virtual Game Game { get; set; }
        public virtual Player Player { get; set; }
        public GameStatus Status { get; set; }

        public GamePlayerStatus()
        {
            Status = GameStatus.Awaiting;
        }
        public GamePlayerStatus(Player player) : this()
        {
            UserId = player.UserId;
            GameId = player.GameId;
            User = player.User;
            Game = player.Game;
            Player = player;
        }
    }
}