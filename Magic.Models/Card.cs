using System.ComponentModel;
using Magic.Models.Helpers;
using Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Magic.Models
{
    public class Card : AbstractExtensions
    {
        public string Id { get; set; }
        public int MultiverseId { get; set; }
        [Required]
        public string Name { get; set; }
        public string SetId { get; set; }
        public CardSet Set { get; set; }
        public int SetNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateReleased { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        public string Artist { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsTapped { get; set; }
        public Rarity Rarity { get; set; }
        public int ConvertedManaCost { get; set; }
        public string Description { get; set; }
        public string Flavour { get; set; }
        public virtual IList<CardType> Types { get; set; }
        public virtual IList<CardManaCost> Colors { get; set; }
        public virtual IList<CardAvailableAbility> Abilities { get; set; }

        public Card()
        {
            IsTapped = false;
            Types = new List<CardType>();
            Colors = new List<CardManaCost>();
            Abilities = new List<CardAvailableAbility>();
        }

        public Card(JObject jCardObject) : this()
        {
            MultiverseId = jCardObject.Property("id").Value<int>();
            Name = jCardObject.Property("name").Value<string>();
            SetId = jCardObject.Property("cardSetId").Value<string>();
            SetNumber = jCardObject.Property("setNumber").Value<int>();
            DateReleased = jCardObject.Property("releasedAt").Value<DateTime>();
            Artist = jCardObject.Property("artist").Value<string>();
            Rarity = (Rarity) Enum.Parse(typeof(Rarity), jCardObject.Property("rarity").Value<string>(), true);
            ConvertedManaCost = jCardObject.Property("convertedManaCost").Value<int>();

            Id = Name.ToLower().Replace(" ", "_").Replace("[^a-z0-9]*", "");
        }
    }

    public class CreatureCard : Card
    {
        public bool IsToken { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }
    }
    
    public class PlaneswalkerCard : Card
    {
        public int Loyalty { get; set; }
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