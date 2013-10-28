using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardColorCost : AbstractToString
    {
        public CardColor CardColorId { get; set; }
        public int Cost { get; set; }
    }
}