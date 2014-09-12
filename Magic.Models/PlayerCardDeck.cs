using System.Collections.Generic;
using Magic.Models.Helpers;

namespace Magic.Models
{
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
}