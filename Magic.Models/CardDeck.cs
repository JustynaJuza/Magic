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
        public int TotalCardNumber { get; set; }
        public List<int> CardsPerTypeNumber { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
        public virtual List<Card> Cards { get; set; }
        public virtual ApplicationUser Creator { get; set; }
    }

    public class CardDeckViewModel : AbstractExtensions, IViewModel
    {
        public int Id { get { return this.Id; } }
        public string Name { get; set; }
        public virtual List<CardViewModel> Cards { get; set; }
        public virtual ApplicationUser Owner { get; set; }
    }
}