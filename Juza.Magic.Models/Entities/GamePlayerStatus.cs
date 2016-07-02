using Juza.Magic.Models.Enums;

namespace Juza.Magic.Models.Entities
{
    public class GamePlayerStatus
    {
        public int UserId { get; set; }
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