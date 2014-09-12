using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Models
{
    public class CardSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Block { get; set; }
        public string Description { get; set; }
        public DateTime ReleasedDate { get; set; }
        public IList<Card> Cards { get; set; } 
    }
}
