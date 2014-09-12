using System.ComponentModel.DataAnnotations;

namespace Magic.Models
{
    public enum Rarity
    {
        Common, 
        Uncommon, 
        Rare,
        [Display(Name="Mythic Rare")]
        MythicRare
    }
}