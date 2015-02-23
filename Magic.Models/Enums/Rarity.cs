using System.ComponentModel.DataAnnotations;

namespace Magic.Models
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