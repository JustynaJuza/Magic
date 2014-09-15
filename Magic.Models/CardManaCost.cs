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

        public bool IsHybrid
        {
            get
            {
                return this is HybridManaCost;
            }
        }
    }

    public class HybridManaCost : CardManaCost
    {
        public int HybridColorId { get; set; }
        public ManaColor HybridColor { get; set; }
    }
}