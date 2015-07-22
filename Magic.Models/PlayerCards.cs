using System;
using System.Linq;
using System.Web;
using Magic.Models.Extensions;

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

    public class PlayerCard : AbstractExtensions
    {
        public string UserId { get; set; }
        public string GameId { get; set; }
        public string CardId { get; set; }
        public int DeckId { get; set; }
        public int Index { get; set; }
        public PlayerCardLocation Location { get; set; }

        //public User User { get; set; }
        //public Game Game { get; set; }
        public Player Player { get; set; }
        public Card Card { get; set; }
        public PlayerCardDeck Deck { get; set; }

        public PlayerCard()
        {
            Location = PlayerCardLocation.Library;
        }
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