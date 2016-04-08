namespace Juza.Magic.Models.Enums
{
    public enum TypeCategory
    {
        SuperType = 1,
        MainType,
        SubType
    }

    public enum SuperType
    {
        Basic = 1,
        Elite,
        Legendary,
        Ongoing,
        Snow,
        World,
    }

    public enum MainType
    {
        Land = 1,
        Creature,
        Instant,
        Sorcery,
        Enchantment,
        Artifact,
        Planeswalker,
        Tribal,
        Conspiracy
        //Interrupt = Instant,
        //Summon = Creature
    }

    //---------- SUBTYPES ----------
    public enum ArtifactType
    {
        Artifact = 1,
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