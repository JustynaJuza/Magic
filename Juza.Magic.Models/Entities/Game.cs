using System;
using System.Collections.Generic;
using System.Linq;
using Juza.Magic.Models.Extensions;
using Juza.Magic.Models.Interfaces;

namespace Juza.Magic.Models.Entities
{
    public class Game : AbstractExtensions
    {
        public string Id { get; set; }
        public bool IsPrivate { get; set; }
        public int PlayerCapacity { get; set; }
        public TimeSpan TimePlayed { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateResumed { get; set; }
        public DateTime? DateEnded { get; set; }
        public IList<UserViewModel> Observers { get; set; }
        public virtual IList<GamePlayerStatus> Players { get; set; }

        public Game()
        {
            Id = Guid.NewGuid().ToString();
            IsPrivate = false;
            PlayerCapacity = 2;
            TimePlayed = new TimeSpan(0);
            Players = new List<GamePlayerStatus>();
            Observers = new List<UserViewModel>();
        }

        public Game(bool isPrivate) : this()
        {
            IsPrivate = isPrivate;
        }

        public Game(IList<Player> players) : this(true)
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

        public void UpdateTimePlayed(DateTime dateSuspended)
        {
            TimePlayed += (dateSuspended - (DateTime)(DateResumed.HasValue ? DateResumed : DateStarted));
        }
    }

    public class GameViewModel : AbstractExtensions, IViewModel
    {
        public string Id { get; set; }
        public bool IsPrivate { get; set; }
        public int PlayerCapacity { get; set; }
        public string TimePlayed { get; set; }
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
            TimePlayed = "00:00:00";
            Players = new List<PlayerViewModel>();
            Observers = new List<UserViewModel>();
        }
        public GameViewModel(Game game) : this()
        {
            Id = game.Id;
            IsPrivate = game.IsPrivate;
            PlayerCapacity = game.PlayerCapacity;
            TimePlayed = game.TimePlayed.ToTotalHoursString();
            Players = new List<PlayerViewModel>();
            foreach (var player in game.Players)
            {
                Players.Add((PlayerViewModel)player.Player.GetViewModel());
            }
            Observers = game.Observers.ToList();
        }
        // Constructor with number of players.
        public GameViewModel(Game game, int playerCount) : this(game)
        {
            PlayerCapacity = playerCount;
        }
    }
}