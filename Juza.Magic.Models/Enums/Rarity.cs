using System.ComponentModel.DataAnnotations;

namespace Juza.Magic.Models.Enums
{
    public enum Rarity
    {
        Common = 1, 
        Uncommon, 
        Rare,
        [Display(Name="Mythic Rare")]
        MythicRare
    }
}