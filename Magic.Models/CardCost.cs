using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardCost : AbstractToString
    {
        public CardColor Color { get; set; }
        public int Cost { get; set; }
    }
}