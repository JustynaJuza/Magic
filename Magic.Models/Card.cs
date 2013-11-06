using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Card : AbstractToString
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The card must have a name.")]
        public string Name { get; set; }
        public CardType CardType { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
        //[NotMapped]
        //public bool Permanent { get; set; }
    }

    public class CardViewModel : AbstractToString
    {
        public int Id { get {return this.Id;} }
        public string Name { get; set; }
        public bool Permanent { get; set; }
        public List<CardColor> CardColors { get; set; }
    }
}