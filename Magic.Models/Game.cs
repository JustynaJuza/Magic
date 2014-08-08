using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Game : AbstractExtensions
    {
        public string Id { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateEnded { get; set; }
        public virtual IList<GameUser> Players { get; set; }

        public Game()
        {
            Id = Guid.NewGuid().ToString();
            IsPrivate = false;
            Players = new List<GameUser>();
        }
        public Game(bool isPrivate) : this()
        {
            Id = Guid.NewGuid().ToString();
            IsPrivate = isPrivate;
            Players = new List<GameUser>();
        }
    }

    public class GameViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public bool IsPrivate { get; set; }
        public int PlayerCapacity { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateEnded { get; set; }
        public virtual List<Player> Players { get; set; }
        public virtual List<User> Observers { get; set; }

        // Constructor.
        public GameViewModel() {
            Id = Guid.NewGuid().ToString();
            PlayerCapacity = 2;
            IsPrivate = false;
            Players = new List<Player>();
            Observers = new List<User>();
        }
        public GameViewModel(Game game) : this()
        {
            Id = game.Id;
            IsPrivate = game.IsPrivate;
        }
        // Constructor with number of players.
        public GameViewModel(Game game, int playerCount) : this(game)
        {
            PlayerCapacity = playerCount;
        }
        // Constructor with players.
        public GameViewModel(Game game, List<Player> players) : this(game)
        {
            PlayerCapacity = players.Count;
            Players = players;
        }
    }
}