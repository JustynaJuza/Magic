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
        // Constructor with number of players.
        public GameViewModel(int playerCount) 
            : this()
        {
            PlayerCapacity = playerCount;
        }
        // Constructor with players.
        public GameViewModel(List<Player> players)
            : this()
        {
            PlayerCapacity = players.Count;
            Players = players;
        }
    }
}