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
        public int PlayerCapacity { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateEnded { get; set; }
        public IList<UserViewModel> Observers { get; set; }
        public virtual IList<GamePlayerStatus> Players { get; set; }

        public Game()
        {
            Id = Guid.NewGuid().ToString();
            IsPrivate = false;
            PlayerCapacity = 2;
            Players = new List<GamePlayerStatus>();
            Observers = new List<UserViewModel>();
        }

        public Game(bool isPrivate) : this()
        {
            IsPrivate = isPrivate;
        }

        public Game(IList<Player> players)
        {
            PlayerCapacity = players.Count;
            Players = new List<GamePlayerStatus>();
            foreach (var player in players)
            {
                Players.Add(new GamePlayerStatus()
                {
                    UserId = player.UserId,
                    Player = player
                });
            }
        }
    }

    public class GameViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public bool IsPrivate { get; set; }
        public int PlayerCapacity { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateEnded { get; set; }
        public virtual List<PlayerViewModel> Players { get; set; }
        public virtual List<UserViewModel> Observers { get; set; }

        // Constructor.
        public GameViewModel()
        {
            Id = Guid.NewGuid().ToString();
            PlayerCapacity = 2;
            IsPrivate = false;
            Players = new List<PlayerViewModel>();
            Observers = new List<UserViewModel>();
        }
        public GameViewModel(Game game)
            : this()
        {
            Id = game.Id;
            IsPrivate = game.IsPrivate;
            PlayerCapacity = game.PlayerCapacity;
            Players = new List<PlayerViewModel>();
            foreach (var player in game.Players)
            {
                Players.Add((PlayerViewModel)player.GetViewModel());
            }
            Observers = game.Observers.ToList();
        }
        // Constructor with number of players.
        public GameViewModel(Game game, int playerCount)
            : this(game)
        {
            PlayerCapacity = playerCount;
        }
    }
}