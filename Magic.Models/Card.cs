using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Card : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        public virtual CardType Type { get; set; }
        public IList<int> CostPerColor { get; set; }
        public virtual IList<CardColor> Colors { get; set; }
        public virtual IList<CardAbility> Abilities { get; set; }
    }

    public class CreatureCard : Card
    {
        public int Power { get; set; }
        public int Toughness { get; set; }
    }


    public class CardViewModel : AbstractExtensions, ICard, IViewModel
    {
        public int Id { get {return this.Id;} }
        [Required(ErrorMessage = "The card must have a name.")]
        public string Name { get; set; }
        public bool Tapped { get; set; }
        public bool Permanent { get; set; }
        public IList<int> CostPerColor { get; set; }
        public virtual IList<CardColor> Colors { get; set; }
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