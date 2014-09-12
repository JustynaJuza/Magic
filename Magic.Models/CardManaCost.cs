using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardManaCost
    {
        public string CardId { get; set; }
        public int ColorId { get; set; }

        public Card Card { get; set; }
        public ManaColor Color { get; set; }
        public int Cost { get; set; }
        public bool IsHybrid { get; set; }
    }

    //public class HybridManaCost : CardManaCost
    //{
    //    public int SecondColorId { get; set; }
    //    public ManaColor SecondColor { get; set; }
    //}
}