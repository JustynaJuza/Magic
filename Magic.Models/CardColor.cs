using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardColor : AbstractToString
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Card> Cards { get; set; }
    }

    public class CardColorViewModel : AbstractToString
    {
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
    }
}