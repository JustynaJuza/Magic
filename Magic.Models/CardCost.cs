using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardCost : AbstractExtensions
    {
        public ManaColor Color { get; set; }
        public int Cost { get; set; }
    }
}