using System;
using System.Collections.Generic;
using Juza.Magic.Models.Extensions;
using Juza.Magic.Models.Interfaces;

namespace Juza.Magic.Models.Entities
{
    public class CardDeck : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public IList<int> CardsPerTypeNumber { get; set; }
        public virtual IList<ManaColor> Colors { get; set; }
        public virtual IList<Card> Cards { get; set; }
        public virtual User Creator { get; set; }
        public virtual IList<User> UsedByUsers { get; set; }
    }

    public class CardDeckViewModel : AbstractExtensions, IViewModel
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public IList<int> CardsPerTypeNumber { get; set; }
        public virtual IList<ManaColor> Colors { get; set; }
        public virtual IList<Card> Cards { get; set; }
        public virtual UserViewModel Creator { get; set; }

        public CardDeckViewModel(CardDeck deck)
        {
            Id = deck.Id;
            Name = deck.Name;
            CardsPerTypeNumber = deck.CardsPerTypeNumber;
            Colors = deck.Colors;
            Cards = deck.Cards;
            Creator = deck.Creator.GetViewModel();
        }
    }
}