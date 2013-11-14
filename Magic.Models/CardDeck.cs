using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardDeck : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public List<int> CardsPerTypeNumber { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
        public virtual List<Card> Cards { get; set; }
        public virtual ApplicationUser Creator { get; set; }
    }

    public class CardDeckViewModel : AbstractExtensions, IViewModel
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public List<int> CardsPerTypeNumber { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
        public virtual List<Card> Cards { get; set; }
        public virtual UserViewModel Creator { get; set; }

        public CardDeckViewModel(CardDeck deck) {
            Id = deck.Id;
            Name = deck.Name;
            CardsPerTypeNumber = deck.CardsPerTypeNumber;
            CardColors = deck.CardColors;
            Cards = deck.Cards;
            Creator = deck.Creator.GetViewModel();
    }
    }
}