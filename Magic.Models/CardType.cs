using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    using Magic.Models.Helpers;
    // Instants and sorceries have the same subtypes.
    using SorceryType = InstantType;

    public class CardType : AbstractExtensions
    {
        public int Id { get; set; }
        public virtual CardMainType MainType { get; set; }
    }

    public class CardMainType : AbstractExtensions
    {
        public int Id { get; set; }
        public MainType Type { get; set; }
        public virtual IList<CardSubType> SubTypes { get; set; }
    }

    public class CardSubType : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<CardMainType> MainTypes { get; set; }
    }
}