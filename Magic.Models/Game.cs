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
        public int Id { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateEnded { get; set; }
        public virtual IList<PlayerGameStatus> Players { get; set; }
    }

    public class GameViewModel : AbstractExtensions, IViewModel
    {
        public int Id { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateEnded { get; set; }
        public virtual IList<Player> Players { get; set; }
        public virtual IList<ApplicationUser> Observers { get; set; }

        // Constructor with number of players.
        public GameViewModel(int playerCount)
        {
            Players = new List<Player>(playerCount);
            Observers = new List<ApplicationUser>();
        }
        // Constructor with players.
        public GameViewModel(IList<Player> players)
        {
            Players = players;
            Observers = new List<ApplicationUser>();
        }
    }
}