using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{    
    public class ManaColor : AbstractExtensions
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public virtual IList<Card> Cards { get; set; }
    }
}