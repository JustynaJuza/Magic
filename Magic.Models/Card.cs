using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Card : AbstractToString
    {
        public int Id { get; set;  }
        public string Name { get; set; }
        public virtual List<CardColor> CardColors { get; set; }
    }

    public class CardViewModel : AbstractToString
    {
        [MaxLength(25, ErrorMessage = "Player name must be no longer than 25 characters."), MinLength(3, ErrorMessage = "Player name must be at least 3 characters long.")]
        public string Name { get; set; }
        public List<CardColor> CardColors { get; set; }
    }
}