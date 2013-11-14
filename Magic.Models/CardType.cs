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
        public virtual List<CardSubType> SubTypes { get; set; }
    }

    public class CardSubType : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<CardMainType> MainTypes { get; set; }
    }


    public enum MainType
    {
        Artifact,
        Creature,
        Enchantment,
        Instant,
        Land,
        Planeswalker,
        Tribal,
        Sorcery
    }

    public enum ArtifactType
    {
        Artifact,
        Creature,
        Enchantment,
        Instant,
        Land,
        Planeswalker,
        Tribal,
        Sorcery
    }

    public enum CreatureType
    {
        Artifact,
        Creature,
        Enchantment,
        Instant,
        Land,
        Planeswalker,
        Tribal,
        Sorcery
    }

    public enum EnchantmentType
    {
        Artifact,
        Creature,
        Enchantment,
        Instant,
        Land,
        Planeswalker,
        Tribal,
        Sorcery
    }

    public enum InstantType
    {
        Artifact,
        Creature,
        Enchantment,
        Instant,
        Land,
        Planeswalker,
        Tribal,
        Sorcery
    }

    public enum LandType
    {
        Forest,
        Island,
        Mountain,
        Plains,
        Swamp
    }

    public enum PlaneswalkerType
    {
        Artifact,
        Creature,
        Enchantment,
        Instant,
        Land,
        Planeswalker,
        Tribal,
        Sorcery
    }
}