using Juza.Magic.Models.Extensions;

namespace Juza.Magic.Models.Entities
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
        public int UserId { get; set; }
        public string GameId { get; set; }
        public int CardId { get; set; }
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