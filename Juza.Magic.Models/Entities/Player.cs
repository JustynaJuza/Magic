using Juza.Magic.Models.Enums;
using Juza.Magic.Models.Interfaces;
using System.Collections.Generic;

namespace Juza.Magic.Models.Entities
{
    public class Player
    {
        private int defaultHealth = 20;

        public string GameId { get; set; }
        public int UserId { get; set; }
        public virtual Game Game { get; set; }
        public virtual User User { get; set; }

        public virtual PlayerCardDeck Deck { get; set; }
        public int HealthTotal { get; set; }
        public int HealthCurrent { get; set; }
        public PlayerStatus Status { get; set; }

        public Player()
        {
            HealthTotal = defaultHealth;
            HealthCurrent = defaultHealth;
        }
    }

    public class PlayerViewModel : IViewModel<Player>
    {
        public UserViewModel User { get; set; }
        public CardDeckViewModel Deck { get; set; }
        public int HealthTotal { get; set; }
        public int HealthCurrent { get; set; }
        public int CardsInLibraryTotal { get; set; }
        public int CardsPlayed { get; set; }
        public List<CardViewModel> Library { get; set; }
        public List<CardViewModel> Graveyard { get; set; }
        public List<CardViewModel> Hand { get; set; }
        public List<CardViewModel> Exiled { get; set; }
        public List<CardViewModel> Battlefield { get; set; }
    }
}