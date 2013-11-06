using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{    
    public class CardColor : AbstractToString
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public virtual List<Card> Cards { get; set; }
    }

    public enum Color
    {
        Colorless,
        Black,
        Blue,
        Green,
        Red,
        White
    }
}