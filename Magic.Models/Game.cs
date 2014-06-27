﻿using Magic.Models.Helpers;
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
        public DateTime? DateStarted { get; set; }
        public DateTime? DateEnded { get; set; }
        public virtual IList<Player_GameStatus> Players { get; set; }
    }

    public class GameViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public int PlayerCapacity { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateEnded { get; set; }
        public virtual List<Player> Players { get; set; }
        public virtual List<ApplicationUser> Observers { get; set; }

        // Constructor.
        public GameViewModel() {
            Id = Guid.NewGuid().ToString();
            PlayerCapacity = 2;
            Players = new List<Player>();
            Observers = new List<ApplicationUser>();
        }
        // Constructor with number of players.
        public GameViewModel(int playerCount)
        {
            Id = Guid.NewGuid().ToString();
            PlayerCapacity = playerCount;
            Players = new List<Player>();
            Observers = new List<ApplicationUser>();
        }
        // Constructor with players.
        public GameViewModel(List<Player> players)
        {
            Id = Guid.NewGuid().ToString();
            PlayerCapacity = players.Count;
            Players = players;
            Observers = new List<ApplicationUser>();
        }
    }
}