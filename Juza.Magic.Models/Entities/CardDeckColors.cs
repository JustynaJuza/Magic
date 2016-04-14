﻿namespace Juza.Magic.Models.Entities
{
    public class CardDeckColors
    {
        public int DeckId { get; set; }
        public int ColorId { get; set; }

        public CardDeck Deck { get; set; }
        public ManaColor Color { get; set; }
    }
}