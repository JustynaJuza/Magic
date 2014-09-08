using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Magic.Models.Helpers;

namespace Magic.Models
{
    public enum PlayerCardLocation
    {
        Battlefield,
        Hand,
        Library,
        Graveyard,
        Exiled
    }
    
    public class PlayerCardDeck : AbstractExtensions
    {
        public int DeckId { get; set; }
        public string UserId { get; set; }
        public string GameId { get; set; }
        //public int CardsTotal { get; set; }
        public int CardsPlayed { get; set; }

        public virtual Player Player { get; set; }
        public virtual CardDeck Deck { get; set; }
        public virtual IList<PlayerCard> Cards { get; set; }

        public PlayerCardDeck()
        {
            CardsPlayed = 0;
            Cards = new List<PlayerCard>();
        }
    }

    public class PlayerCard : AbstractExtensions
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string CardId { get; set; }
        public int DeckId { get; set; }

        //public User User { get; set; }
        //public Game Game { get; set; }
        public Player Player { get; set; }
        public Card Card { get; set; }
        public PlayerCardDeck Deck { get; set; }
    }
    
    public class PlayerCardBattlefield : PlayerCard
    {

    }

    public class PlayerCardHand : PlayerCard
    {

    }

    public class PlayerCardLibrary : PlayerCard
    {

    }

    public class PlayerCardGraveyard : PlayerCard
    {

    }

    public class PlayerCardExiled : PlayerCard
    {

    }
}