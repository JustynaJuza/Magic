using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardDeck
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalCardNumber { get; set; }
        public List<int> CardsPerTypeNumber { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
        public virtual List<Card> Cards { get; set; }
        public virtual ApplicationUser Owner { get; set; }
    }

    public class CardDeckViewModel
    {
        public int Id { get { return this.Id; } }
        public string Name { get; set; }
        public virtual List<CardViewModel> Cards { get; set; }
        public virtual ApplicationUser Owner { get; set; }


        public IEnumerable<CardViewModel> Shuffle(Random random)
        {
            var deck = Cards.ToArray();
            for (int i = deck.Length - 1; i >= 0; i--)
            {
                // Swap with random element and lazily return.
                int swapIndex = random.Next(i + 1);
                yield return deck[swapIndex];
                deck[swapIndex] = deck[i];
            }
        }
    }
}