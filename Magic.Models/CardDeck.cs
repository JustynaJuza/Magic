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
        public virtual List<Card> Cards { get; set; }
        public virtual ApplicationUser Owner { get; set; }
    }
}