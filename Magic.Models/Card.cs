using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Magic.Models
{
    public class Card : AbstractExtensions
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        [Required]
        public string Name { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        public virtual CardType Type { get; set; }
        public IList<int> CostPerColor { get; set; }
        public virtual IList<ManaColor> Colors { get; set; }
        public virtual IList<CardAbility> Abilities { get; set; }
    }

    public class CreatureCard : Card
    {
        public int Power { get; set; }
        public int Toughness { get; set; }
    }

    public class CardViewModel : AbstractExtensions, ICard, IViewModel
    {
        public string Id { get {return this.Id;} }
        [Required(ErrorMessage = "The card must have a name.")]
        public string Name { get; set; }
        public bool Tapped { get; set; }
        public bool Permanent { get; set; }
        public IList<int> CostPerColor { get; set; }
        public virtual IList<ManaColor> Colors { get; set; }
        public virtual IList<CardAbility> Abilities { get; set; }

        public void Tap()
        {
            Tapped = true;
        }
        public void UnTap()
        {
            Tapped = false;
        }
        public bool Play() { return true; }

        public CardViewModel()
        {

        }
    }
}