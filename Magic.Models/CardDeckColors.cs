using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardDeckColors
    {
        public int DeckId { get; set; }
        public int ColorId { get; set; }

        public CardDeck Deck { get; set; }
        public ManaColor Color { get; set; }
    }
}