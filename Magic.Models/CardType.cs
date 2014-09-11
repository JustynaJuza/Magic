using System.Collections.Generic;
using Magic.Models.Helpers;

namespace Magic.Models
{
    // Instants and sorceries have the same subtypes.
    using SorceryType = InstantType;
    
    public class CardType : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Card> Cards{ get; set; }
    }

    public class CardSuperType : CardType { }

    public class CardMainType : CardType { }

    public class CardSubType : CardType { }

    public class CreatureCardSubType : CardSubType
    {
        public bool IsRace { get; set; }
    }
}